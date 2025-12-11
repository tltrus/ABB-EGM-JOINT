using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using Abb.Egm;
using Google.Protobuf.Collections;
using System.Threading;

namespace WpfApp.ViewModels
{
    public class RobotController : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;

        private UdpClient udpClient;
        private IPEndPoint robotEndpoint;
        private uint sequenceNumber = 0;
        private bool isConnected = false;
        private Thread sendThread;
        private Thread receiveThread;
        private bool keepSending = false;
        private double[] currentTarget = new double[6];
        private double[] currentPosition = new double[6];
        private double[] currentTcpPosition = new double[3]; // X, Y, Z
        private bool targetReached = false;
        private object lockObject = new object();
        private double tolerance = 0.3; // Tolerance in degrees
        private bool isFirstMovement = true;
        private int localPort;
        private string robotIP;

        // Events for UI updates
        public event Action<double[]> PositionUpdated;
        public event Action<double[]> TcpPositionUpdated;
        public event Action<string> StatusMessage;

        public RobotController(string robotIP, int robotPort, int localPort = 6510)
        {
            this.robotIP = robotIP;
            this.localPort = localPort;
            robotEndpoint = new IPEndPoint(IPAddress.Parse(robotIP), robotPort);

            // Initialize arrays
            currentTcpPosition = new double[3] { 0, 0, 0 };
            currentPosition = new double[6] { 0, 0, 0, 0, 0, 0 };
            currentTarget = new double[6] { 0, 0, 0, 0, 0, 0 };
        }

        // RECEIVE DATA
        public void StartReading()
        {
            StatusMessage?.Invoke("Starting robot data reading...");

            try
            {
                // Create new UDP client if needed
                if (udpClient == null)
                {
                    udpClient = new UdpClient(localPort);
                    udpClient.Client.ReceiveTimeout = 5000;
                }

                keepSending = true;

                // Stop existing threads if they're running
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(100);
                }

                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                isConnected = true;
            }
            catch (Exception ex)
            {
                StatusMessage?.Invoke($"Error starting reading: {ex.Message}");
                isConnected = false;
                throw;
            }
        }

        private void ReceiveData()
        {
            while (keepSending)
            {
                try
                {
                    byte[] receivedData = udpClient.Receive(ref robotEndpoint);
                    var robotMsg = EgmRobot.Parser.ParseFrom(receivedData);

                    // Process joint data
                    if (robotMsg.FeedBack?.Joints != null && robotMsg.FeedBack.Joints.Joints.Count >= 6)
                    {
                        // Update current joint position with rounding
                        for (int i = 0; i < 6; i++)
                        {
                            var joint = robotMsg.FeedBack.Joints.Joints[i];
                            if (joint < 0.009 && joint > -0.009) // fix -0.00 displaying
                                currentPosition[i] = 0;
                            else
                                currentPosition[i] = Math.Round(robotMsg.FeedBack.Joints.Joints[i], 2);
                        }

                        // Call joint position update event
                        PositionUpdated?.Invoke((double[])currentPosition.Clone());

                        // Check target achievement only if movement is active
                        if (!targetReached && IsPositionReached(currentTarget, currentPosition))
                        {
                            targetReached = true;
                            StatusMessage?.Invoke($"Target position reached! Axes: {FormatPositions(currentPosition)}");
                        }
                    }

                    // Process TCP data
                    if (robotMsg?.FeedBack?.Cartesian?.Pos != null)
                    {
                        var pos = robotMsg.FeedBack.Cartesian.Pos;

                        // Update TCP position (in mm, convert from meters if needed)
                        currentTcpPosition[0] = Math.Round(pos.X, 2); // Convert meters to mm
                        currentTcpPosition[1] = Math.Round(pos.Y, 2);
                        currentTcpPosition[2] = Math.Round(pos.Z, 2);

                        // Call TCP position update event
                        TcpPositionUpdated?.Invoke((double[])currentTcpPosition.Clone());
                    }

                    // Ignore MciConvergenceMet on first movement or if target already reached
                    if (robotMsg.MciConvergenceMet && !targetReached && !isFirstMovement)
                    {
                        targetReached = true;
                        StatusMessage?.Invoke("EGM reports target achievement");
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    continue;
                }
                catch (SocketException ex)
                {
                    if (keepSending) // Only log if we're still supposed to be running
                    {
                        StatusMessage?.Invoke($"Socket error: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    if (keepSending)
                    {
                        StatusMessage?.Invoke($"Receive error: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public double[] GetCurrentPosition() => (double[])currentPosition.Clone();

        public double[] GetCurrentTcpPosition() => (double[])currentTcpPosition.Clone();


        // SEND DATA
        public void StartControl()
        {
            StatusMessage?.Invoke("Starting robot control...");

            Array.Copy(currentPosition, currentTarget, currentTarget.Length);

            // Start send thread if not already running
            if (sendThread == null || !sendThread.IsAlive)
            {
                sendThread = new Thread(ContinuousSend);
                sendThread.IsBackground = true;
                sendThread.Start();
            }

            Thread.Sleep(500);
            StatusMessage?.Invoke("Ready to send commands");
        }

        public bool MoveToPosition(double[] joints, int timeoutMs = 5000)
        {
            if (joints.Length != 6)
            {
                StatusMessage?.Invoke("Error: array of 6 values required");
                return false;
            }

            // Check if new target matches current position
            if (IsPositionReached(joints, currentPosition) && !isFirstMovement)
            {
                StatusMessage?.Invoke("Robot is already at target position");
                targetReached = true;
                return true;
            }

            // Set new target position
            Array.Copy(joints, currentTarget, 6);
            targetReached = false;

            StatusMessage?.Invoke($"Starting movement to position: {FormatPositions(joints)}");
            StatusMessage?.Invoke($"Current position: {FormatPositions(currentPosition)}");

            // Reset first movement flag after real movement starts
            if (isFirstMovement)
            {
                isFirstMovement = false;
            }

            // Wait for target achievement with timeout
            DateTime startTime = DateTime.Now;
            bool positionVisuallyReached = false;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                // Check if current position is reached
                if (IsPositionReached(currentTarget, currentPosition))
                {
                    if (!positionVisuallyReached)
                    {
                        StatusMessage?.Invoke($"Visual position achievement: {FormatPositions(currentPosition)}");
                        positionVisuallyReached = true;
                    }

                    // Wait for EGM confirmation or additional timeout
                    if (targetReached || (DateTime.Now - startTime).TotalMilliseconds > 1000)
                    {
                        StatusMessage?.Invoke("Target position reached!");
                        return true;
                    }
                }

                Thread.Sleep(100);
            }

            if (!targetReached)
            {
                StatusMessage?.Invoke($"Timeout! Movement not completed in {timeoutMs}ms");
                StatusMessage?.Invoke($"Current position: {FormatPositions(currentPosition)}");
                return false;
            }

            return true;
        }

        private bool IsPositionReached(double[] target, double[] current)
        {
            for (int i = 0; i < 6; i++)
            {
                double diff = Math.Abs(target[i] - current[i]);
                if (diff > tolerance)
                    return false;
            }
            return true;
        }

        private string FormatPositions(double[] positions)
        {
            string[] formatted = new string[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                formatted[i] = Math.Round(positions[i], 2).ToString("F2");
            }
            return $"[{string.Join(", ", formatted)}]";
        }

        private void ContinuousSend()
        {
            while (keepSending)
            {
                try
                {
                    if (isConnected && udpClient != null)
                    {
                        var sensorMsg = new EgmSensor
                        {
                            Header = new EgmHeader
                            {
                                Seqno = sequenceNumber++,
                                Tm = (uint)DateTime.Now.Ticks,
                                Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection
                            },
                            Planned = new EgmPlanned
                            {
                                Joints = new EgmJoints()
                            }
                        };

                        // Send current target position
                        lock (lockObject)
                        {
                            foreach (var joint in currentTarget)
                            {
                                sensorMsg.Planned.Joints.Joints.Add(joint);
                            }
                        }

                        byte[] data = sensorMsg.ToByteArray();
                        udpClient.Send(data, data.Length, robotEndpoint);
                    }

                    Thread.Sleep(4); // ~250Hz
                }
                catch (Exception ex)
                {
                    if (keepSending)
                    {
                        StatusMessage?.Invoke($"Send error: {ex.Message}");
                        Thread.Sleep(10);
                    }
                }
            }
        }

        // STOP and cleanup for reconnection
        public void Stop()
        {
            keepSending = false;
            isConnected = false;
            targetReached = true; // Reset target flag

            // Give threads time to stop
            sendThread?.Join(500);
            receiveThread?.Join(500);

            // Clean up UDP client
            if (udpClient != null)
            {
                try
                {
                    udpClient.Close();
                    udpClient = null; // Allow recreation
                }
                catch (Exception ex)
                {
                    StatusMessage?.Invoke($"Error closing UDP client: {ex.Message}");
                }
            }

            // Reset threads
            sendThread = null;
            receiveThread = null;

            StatusMessage?.Invoke("Control stopped");
        }

        // Check if controller is active
        public bool IsActive() => isConnected && keepSending;

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            udpClient?.Dispose();

            sendThread?.Join(1000);
            receiveThread?.Join(1000);

            GC.SuppressFinalize(this);
        }

        ~RobotController()
        {
            Dispose();
        }
    }
}