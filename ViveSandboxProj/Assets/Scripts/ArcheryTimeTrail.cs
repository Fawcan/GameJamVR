using UnityEngine;
using System.Collections;

public class ArcheryTimeTrail : MonoBehaviour
{
    [System.Serializable]
    struct RandomData
    {
        public float Max;
        public float Min;
    }

    [System.Serializable]
    struct RandomSettingsData
    {
        public Transform StartPosition;
        public RandomData VerticalAngle;
        public RandomData HorizontalAngle;
        public RandomData Distance;
    }

    [System.Serializable]
    struct ScoreSettingsData
    {
        public TextMesh ScoreText;
        public TextMesh HitsText;

        public string ScorePreText;
        public string HitsPreText;

        public float ScoreOnScreen;
    }

    [SerializeField]
    private GameObject[] DisableObjectsOnStart;
    [SerializeField]
    private MonoBehaviour[] DisableScripsOnStart;
    [SerializeField]
    private TargetBoard StartTrigger;
    [SerializeField]
    private ScoreSettingsData ScoreSettings;
    [SerializeField]
    private TargetBoard TargetBoardPrefab;
    [SerializeField]
    private RandomSettingsData RandomSettings;
    [SerializeField]
    private float PlayTime;


    struct StateData
    {
        public int LastStartTrigArrowCount;

        public TargetBoard SpawnedTarget;

        public int Hits;
        public float Score;
        public float StartTime;
        public float ScoreShowTime;

        public bool InGame;
    }
    StateData _State;
    
    void Start ()
    {
        _State.LastStartTrigArrowCount = StartTrigger.ArrowHits;
        _State.InGame = false;
    }
	
	void Update ()
    {
        if (_State.InGame)
        {
            if ((_State.StartTime + PlayTime) < Time.time)
            {
                _State.ScoreShowTime = Time.time;
                ScoreSettings.ScoreText.text = ScoreSettings.ScorePreText + System.Math.Round(_State.Score, 1).ToString();
                ScoreSettings.HitsText.text = ScoreSettings.HitsPreText + _State.Hits.ToString();
                ScoreSettings.ScoreText.gameObject.SetActive(true);
                ScoreSettings.HitsText.gameObject.SetActive(true);
                _State.InGame = false;

                Destroy(_State.SpawnedTarget.gameObject);

                SetObjectsAndCompActive(true);

                return;
            }

            if (_State.SpawnedTarget == null)
                _State.SpawnedTarget = CreateNewRandomTargetBoard();
            else
            {
                if (_State.SpawnedTarget.ArrowHits > 0)
                {
                    _State.Hits += _State.SpawnedTarget.ArrowHits;
                    _State.Score += _State.SpawnedTarget.ArrowHits * _State.SpawnedTarget.LastHitScore;

                    _State.SpawnedTarget.Remove();
                    _State.SpawnedTarget = CreateNewRandomTargetBoard();
                }
            }
        }
        else
        {
            if (ScoreSettings.ScoreText.gameObject.activeSelf && (_State.ScoreShowTime + ScoreSettings.ScoreOnScreen) < Time.time)
            {
                StartTrigger.gameObject.SetActive(true);
                ScoreSettings.ScoreText.gameObject.SetActive(false);
                ScoreSettings.HitsText.gameObject.SetActive(false);
            }

            if (StartTrigger.ArrowHits > _State.LastStartTrigArrowCount)
            {
                _State.InGame = true;
                _State.StartTime = Time.time;
                _State.Score = 0;
                _State.Hits = 0;

                SetObjectsAndCompActive(false);
                StartTrigger.gameObject.SetActive(false);
            }

            _State.LastStartTrigArrowCount = StartTrigger.ArrowHits;
        }
    }

    void SetObjectsAndCompActive(bool active)
    {
        foreach (var item in DisableObjectsOnStart)
            item.SetActive(active);
        foreach (var item in DisableScripsOnStart)
            item.enabled = active;
    }

    TargetBoard CreateNewRandomTargetBoard()
    {
        float verticalAngle = Random.Range(RandomSettings.VerticalAngle.Min, RandomSettings.VerticalAngle.Max);
        float horizontalAngle = Random.Range(RandomSettings.HorizontalAngle.Min, RandomSettings.HorizontalAngle.Max);
        float distance = Random.Range(RandomSettings.Distance.Min, RandomSettings.Distance.Max);

        var board = (TargetBoard)Instantiate(TargetBoardPrefab, RandomSettings.StartPosition.position, RandomSettings.StartPosition.rotation);
        var boardTrans = board.transform;
        boardTrans.Rotate(new Vector3(-verticalAngle, horizontalAngle), Space.Self);
        boardTrans.Translate(new Vector3(0, 0, distance), Space.Self);
        boardTrans.forward = (RandomSettings.StartPosition.position - boardTrans.position);

        return board;
    }
}