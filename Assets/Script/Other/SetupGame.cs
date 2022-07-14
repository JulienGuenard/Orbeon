using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetupGame : NetworkBehaviour
{

    public BallonData ballonPrefab;
    public Vector3 ballonPos;
  public Vector3 ballonScale;

    public override void OnStartServer()
    {
          StartCoroutine(waitForInit());
    }

    IEnumerator waitForInit()
    {
        while (!LoadingManager.Instance.isGameReady())
            yield return new WaitForSeconds(0.01f);
        Init();
    }

    private void Init()
    {
        GameObject ballon = Instantiate(ballonPrefab.gameObject, ballonPos, Quaternion.identity);
    ballon.name = "Ballon";
        ballon.transform.localScale = ballonScale;
        NetworkServer.Spawn(ballon);
      RpcFunctions.Instance.CmdSpawnPlayers();
    }
}
