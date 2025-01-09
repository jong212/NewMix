using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memo
{
    ////////////////////////////////////////Common Memo//////////////////////////////////////

  
    // 어드레서블 초기화 시점은 2Down Scene 임 
    // 모든 라벨은 2Down Scene의 DownManager 오브젝트의 DownManager.cs 의 PatchFiles()와CheckUpdateFiles() 메서드 함수 정의 부분의 라벨을 따로 추가해야 게임 시작 단계에서 AWS 리소스파일 가져온거에서 체크된 라벨에 해당하는 파일을 가져올 수 있음. 
    // Statu Authoriy 권한 변경 : 방장 클라가 나가면 자동으로 기존 클라중 한 명이 마스터 클라이언트가 되지만 오브젝트의 StatuAtuthority 옵션은 자동으로 체크 되지 않는다 GameManager.cs에서 AssignMasterClientAuthority 에 등록한 클래스에 한해서만 권한이 바뀜 따라서 방장 클라에서만 StatuAuthoiry가 필요한 경우 방장이 나가는 경울를 대비하여 GameManager AssignMasterClientAuthority 메서드에 클래스를 등록한다.
    // OnChangedRender 사용법 메모 : Networked 변수값이 변경되면 다른 클라(내 PC의 플레이어가 아닌 나른 PC에서의 나의 플레이어 복제본을 다른 클라라고 지칭함)에서의 내 플레이어 스크립트에서도 Networked값이 동기화 되는데 OnChangedRender 에 설정한 메서드도 같이 호출 됨 

    // 인벤토리 구현
    // https://www.youtube.com/watch?v=74vxsqQsFHE
    // 스프라이트 어드레서블 구현 해놨음


    ////////////////////////////////////////Scene Info//////////////////////////////////////
    // 1Lobby 
    // - 버튼 클릭 씨 다음씬 로드

    // 2Down
    // - DownManager : 서버에서 다운 받을 파일이 있는지 체크 및 다운
    // - DownManager : 다운 받을 게 있으면 다운 받고 바로 4Login Scene 변환 하고 다운 받을 게 없으면 3Loading 씬을 거쳐 2초 대기 후 4Login Scene 변환 

    // 4Login
    // 1. LoginSceneManager.cs : StaticManager 로드 => UIManager 초기화
    // 2. LoginSceneManager.cs : 서버에서 차트 다운로드 받아서 최신화함
    // 4Login에서 스태틱 매니저 인스턴스화 하고 인스턴스화 된 스태틱 매니저에서 UIManager 초기화

    // BackendGameData.cs 
    // 역할 : 설계도 클래스 모음용 스크립트, 캐릭터 생성 관련 비즈니스 로직 살짝 포함 됨
    // 메모리 : Mono 상속 x , 스태틱 인스턴스 O , 씬 전환시 메모리 유지
    // 순서  모노 상속 안 한 설계도 모음이라 순서 상관 x

    // 개선 리스트
    // 250106 - FPS 떨어지는 현상 있어서 보니 DrowGrid? 그거 선그려주는것 때문에 그랬음 주석 처리 하니 상승했음
    // 250106 - 레벨업, 아이텝드랍 처리를 동기에서 비동기로 변경하니 끊김 현상이 해결 됨
    // 
}
