MODULE Module1
    VAR egmident egm_id_joint;
    LOCAL CONST jointtarget home_pos := [[0, 0, 0, 0, 30, 0], [9E9, 9E9, 9E9, 9E9, 9E9, 9E9]];
    PERS wobjdata egm_wobj := [FALSE, TRUE, "", [[0, 0, 0], [1, 0, 0, 0]], [[0, 0, 0], [1, 0, 0, 0]]];
    PERS tooldata current_tool := [TRUE, [[0, 0, 0], [1, 0, 0, 0]], [0.001, [0, 0, 0.001], [1, 0, 0, 0], 0, 0, 0]];
    
    ! ??????? ?????????
    LOCAL CONST egm_minmax joint_limits := [-0.1, 0.1];
    LOCAL CONST egm_minmax pose_limits := [-0.5, 0.5];

    PROC main()
        MoveAbsJ home_pos, v200, fine, current_tool;
        InitJointEGM;
        WHILE TRUE DO
            ! Joint EGM (???? 6510)
			TPWrite "?????? EGM...";
            EGMRunJoint egm_id_joint, EGM_STOP_HOLD, 
                        \J1 \J2 \J3 \J4 \J5 \J6 
                        \CondTime:=0.1 
                        \RampInTime:=0.05 
                        \RampOutTime:=0.5;
            WaitTime 0.01;
        ENDWHILE
    ENDPROC
    
    PROC InitJointEGM()
        EGMReset egm_id_joint;
        EGMGetId egm_id_joint;
        
        EGMSetupUC ROB_1, egm_id_joint, "default", "UCdevice1", \Joint \CommTimeout:=5.0;
        
        EGMActJoint egm_id_joint,
                    \WObj:=egm_wobj,
                    \J1:=joint_limits
                    \J2:=joint_limits
                    \J3:=joint_limits
                    \J4:=joint_limits
                    \J5:=joint_limits
                    \J6:=joint_limits
                    \LpFilter:=10 
                    \SampleRate:=4
                    \MaxPosDeviation:=100
                    \MaxSpeedDeviation:=500;
                    
    ENDPROC
    
    PROC StopAll()
        EGMReset egm_id_joint;
    ENDPROC
ENDMODULE