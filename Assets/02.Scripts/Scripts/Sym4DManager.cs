﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sym4D;
using Valve.VR;

public class Sym4DManager : MonoBehaviour
{
    #region Values
    [Header ("Values")]
    public SteamVR_Input_Sources rightHand;
    public SteamVR_Action_Pose rPos;

    public float pitch; //컨트롤러에서 따온 pitch
    public float roll;  //컨트롤러에서 따온 roll

    public int pitchInt;//따온 pitch를 정수형 변환
    public int rollInt; //따온 roll을 정수형 변환

    public int xPort;   //의자 포트 받는 변수
    public int wPort;   //팬 포트 받는 변수
    public bool XConfigCheck;   //의자 포트 설정 붙었는지 확인하는 변수
    public bool WConfigCheck;   //팬 포트 설정 붙었는지 확인하는 변수

    public bool startSetChairCheck = false; //의자 돌리는 코루틴 한번만 실행하게 만드는 변수

    private readonly WaitForSeconds ws = new WaitForSeconds(0.1f);
    #endregion

    IEnumerator Start()
    {
        //의자 포트번호 따오기
        xPort = Sym4DEmulator.Sym4D_X_Find();
        yield return ws;

        //팬 포트번호 따오기
        wPort = Sym4DEmulator.Sym4D_W_Find();
        yield return ws;

        //의자 Roll, Pitch 최대각도 설정
        XConfigCheck = Sym4DEmulator.Sym4D_X_SetConfig(100, 100);
        yield return ws;

        //팬 최대회전수 (최대 100)
        WConfigCheck = Sym4DEmulator.Sym4D_W_SetConfig(100);
        yield return ws;

        //정상적으로 모두 실행되었으면 코루틴 종료
        if(xPort != 0 && wPort != 0 && XConfigCheck == true && WConfigCheck == true)
        {
            print("Sym4D Setting Complete.");
            yield break;
        }
    }

    /// <summary>
    /// Sym4D에서 pitch값과 roll 값만을 사용하여 개발을 진행했다.
    /// 1번 업로드 : 컨트롤러와 의자 회전이 연동되도록 만들었다. 테스트 씬에서 만든 스크립트라 정리가 덜 된 상태
    /// </summary>
    
    // Update is called once per frame
    void Update()
    {
        //Sym4D의 회전값은 10도가 최대이며 값은 -100에서 100까지를 받도록 되어있다.
        if (rPos.localRotation.x >= Mathf.Epsilon - 0.8 && rPos.localRotation.x <= Mathf.Epsilon)
        {
            //-0.8에서 0 사이의 값을 받아서 -100에서 100 사이의 값으로 변환
            pitch = ((Mathf.Clamp(rPos.localRotation.x, Mathf.Epsilon - 0.8f, Mathf.Epsilon)) + 0.4f) / 4 * 1000;
        }
        if (rPos.localRotation.z >= Mathf.Epsilon - 0.7f && rPos.localRotation.z <= Mathf.Epsilon + 0.7f)
        {
            //-0.7에서 0.7 사이의 값을 받아서 -100에서 100 사이의 값으로 변환
            roll = Mathf.Clamp(rPos.localRotation.z, Mathf.Epsilon - 0.7f, Mathf.Epsilon + 0.7f) / 7 * 1000;
        }

        //Sym4D 회전값으로 쓸 정수값으로 컨트롤러 회전값 변환
        pitchInt = (int)pitch;
        rollInt = (int)roll;
        
        //의자 포트가 붙었을 때 코루틴 실행
        if(xPort != 0)
        {
            if(startSetChairCheck == false)
            {
                startSetChairCheck = true;              //의자돌리기 한번만 하도록 값 바꿔버림
                StartCoroutine(SetChairAngle());        //포트 붙었으니 의자 돌리기 시작
            }
        }
    }

    IEnumerator SetChairAngle()
    {
        //print("Called");

        Sym4DEmulator.Sym4D_X_StartContents(xPort);
        yield return ws;

        Sym4DEmulator.Sym4D_X_SendMosionData(rollInt, pitchInt);
        yield return ws;

        //print("After");

        yield return SetChairAngle();
    }

    //삭제될 때 포트 접속 종료(?)
    private void OnDestroy()
    {
        Sym4DEmulator.Sym4D_X_EndContents();
        Sym4DEmulator.Sym4D_W_EndContents();
    }
}
