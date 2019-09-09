using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KlykkLogyc : MonoBehaviour
{
    public Material StartMaterial;
    public Material EndMaterial;
    public Material HiLiteMaterial;

    private static readonly System.Random _random = new System.Random();

    private KlykkitChanimation _chanimation;

    private List<GameObject> _activePiecesList = new List<GameObject>();
    private List<GameObject> _deathPiecesList = new List<GameObject>();

    private GameObject _board;
    private GameObject _piece;
    private GameObject _checkPointer;
    private GameObject _resetButton;

    private Transform _boardForm;
    private Transform _bigTextForm;

    private Camera _camerae;

    private Ray _cast;

    private RaycastHit _me;

    private ParticleSystem _system;
    private ParticleSystem _systemBoard;

    private MeshRenderer _objectRenderer;
    private MeshRenderer _streakRenderer;

    private Material _objectMaterial;
    private Material _streakMaterial;

    private Color32 _streakColour = new Color32(0xCC, 0xFF, 0x00, 0xFF);
    private Color32 _goodColour = new Color32(0x00, 0xCC, 0xFF, 0xFF);

    private TextMesh _level;
    private TextMesh _score;
    private TextMesh _streak;
    private TextMesh _hiscore;
    private TextMesh _toplevel;
    private TextMesh _bigText;
    private TextMesh _bigTextSub;

    private string _colliderName = null;

    private float _scoreTimer = 0f;

    private const int _maxLevels = 100;
    private int _currentLevel = 1;
    private int _topLevel = 1;
    
    private int _checkPointLevel = 0;
    private int _currentScore = 0;
    private int _hiScore = 0;
    private int _streakCount = 0;
    private int _activePieces = 0;
    private int _boardSize = 3;
    private int _counter = 0;

    private bool _entered = false;
    private bool _enterSetup = false;
    private bool _gameOver = false;
    private bool _cycling = false;
    private bool _rotating = false;
    private bool _flipping = false;
    private bool _streaking = false;
    private bool _awaiting = false;
    private bool _checkPointed = false;

    private IEnumerator routine;
    private IEnumerator streakRoutine;
    private IEnumerator rotateRoutine;
    private IEnumerator flipRoutine;
    private IEnumerator animationRoutine;

    private IEnumerator AnimateHer(int magicNumber = 0, float seconds = 0.5f)
    {
        _awaiting = true;

        _chanimation.MagicNumber = magicNumber;

        Debug.Log("AnimateHer: " + _chanimation.MagicNumber + " = " + magicNumber);

        WaitForSeconds awaitHer = new WaitForSeconds(seconds);

        yield return awaitHer;

        if(!_cycling)
        {
            _chanimation.MagicNumber = 0;
        }

        Debug.Log("Stopped animating her at " + Time.timeSinceLevelLoad);

        _awaiting = false;
    }

    private IEnumerator FlipBoard()
    {
        _flipping = true;

        if(animationRoutine != null && _awaiting)
        {
            StopCoroutine(animationRoutine);
            _awaiting = false;
        }

        _chanimation.MagicNumber = 5;

        WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

        Vector3 flip = _boardForm.localScale;

        if(Random.Range(0f, 1f) >= 0.5f)
        {
            flip.x = flip.x < 0f ? 1f : -1f;
        }
        else
        {
            flip.y = flip.y < 0f ? 1f : -1f;
        }

        Debug.Log("<b><color=blue>" + flip + "</color></b>");

        for(float t = 0; t < 1f; t += 4f * Time.deltaTime)
        {
            _boardForm.localScale = Vector3.Lerp(_boardForm.localScale, flip, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return frameWait;
        }

        _boardForm.localScale = flip;

        _chanimation.MagicNumber = 0;

        _flipping = false;
    }

    private IEnumerator RotateBoard()
    {
        _rotating = true; 

        if(animationRoutine != null && _awaiting)
        {
            StopCoroutine(animationRoutine);
            _awaiting = false;
        }

        _chanimation.MagicNumber = 6;

        WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

        Vector3 rotation = _boardForm.eulerAngles;

        rotation.z += Random.Range(0f, 1f) > 0.5f ? 90f : -90f;

        Debug.Log("<b><color=red>" + rotation.z + "</color></b>");

        for(float t = 0; t < 1f; t += 4f * Time.deltaTime)
        {
            _boardForm.rotation = Quaternion.Slerp(_boardForm.rotation, Quaternion.Euler(rotation), Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return frameWait;
        }

        _boardForm.transform.rotation = Quaternion.Euler(rotation);

        _chanimation.MagicNumber = 0;

        _rotating = false;
    }

    private IEnumerator Streaking()
    {
        _streaking = true;

        WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

        for(float t = 0; t < 1f; t += 10f * Time.deltaTime)
        {
            _streakMaterial.color = Color.Lerp(_streakMaterial.color, _streakColour, t * t);
            yield return frameWait;
        }

        for(float t = 0; t < 1f; t += 0.55f * Time.deltaTime)
        {
            _streakMaterial.color = Color.Lerp(_streakColour, Color.white, t);
            yield return frameWait;
        }

        _streakMaterial.color = Color.white;

        _streaking = false;
    }

    private IEnumerator CycleBoard()
    {
        _cycling = true;

        Debug.Log("CYCLING");

        WaitForEndOfFrame frameWait = new WaitForEndOfFrame();
        WaitForSeconds secondsWait1 = new WaitForSeconds(1);
        WaitForSeconds secondsWait2 = new WaitForSeconds(2);

        bool level = _currentLevel >= _maxLevels;

        Vector3 position = Vector3.zero;
        position.y = 5.5f;

        Vector3 bigPosition = Vector3.zero;
        bigPosition.y = -5.5f;

        _chanimation.MagicNumber = 1;

        _bigText.text = "Congratulations!";
        _bigTextSub.text = level ? "You completed the game!" : "You cleared level " + (_currentLevel).ToString();        

        _systemBoard.Play();

        for(float t = 0; t < 1f; t += Time.deltaTime)
        {
            _boardForm.position = Vector3.Lerp(_boardForm.position, position, t * t);
            _bigTextForm.position = Vector3.Lerp(bigPosition, Vector3.zero, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return frameWait;
        }

        _systemBoard.Stop();

        if(!level)
        {
            _chanimation.MagicNumber = 7;

            yield return secondsWait1;

            _chanimation.MagicNumber = 0;
            
            position.y = -5.5f;

            _boardForm.position = Vector3.zero;

            bool cool = _currentLevel % ((float)_maxLevels / 10f) == 0;

            if(cool && _boardSize < 5)
            {
                ++_boardSize;
            }

            SetupBoard(size: _boardSize, reset: false);

            bigPosition.y = 5.5f;

            for(float t = 0; t < 1f; t += Time.deltaTime)
            {
                _boardForm.position = Vector3.Lerp(position, Vector3.zero, Mathf.Sin(t * Mathf.PI * 0.5f));
                _bigTextForm.position = Vector3.Lerp(_bigTextForm.position, bigPosition, t * t);
                yield return frameWait;
            }        

            _bigText.text = _bigTextSub.text = null;

            ++_currentLevel;
            _level.text = _currentLevel.ToString();
            
            _checkPointed = cool;

            if(_checkPointed)
            {
                _checkPointLevel = _currentLevel;
                _checkPointer.SetActive(true);
                _checkPointer.transform.GetChild(0).GetComponent<TextMesh>().text = _checkPointLevel.ToString();
            }
        }
        else
        {
            GameOver(true);
        }

        Debug.Log("CYCLING ENDED");

        _cycling = false;
    }

    void Setup()
    {
        _resetButton = GameObject.Find("Reset");
        _resetButton.SetActive(false);
        _chanimation = FindObjectOfType<KlykkitChanimation>();
        _camerae = FindObjectOfType<Camera>();
        _checkPointer = GameObject.Find("CheckPoint");
        _checkPointer.SetActive(false);
        _bigText = GameObject.Find("BigText").GetComponent<TextMesh>();
        _bigTextSub = _bigText.transform.GetChild(0).GetComponent<TextMesh>();
        _bigTextForm = _bigText.transform.root;
        _level = GameObject.Find("Level").transform.GetChild(0).GetComponent<TextMesh>();
        _score = GameObject.Find("Score").transform.GetChild(0).GetComponent<TextMesh>();
        _toplevel = GameObject.Find("TopLevel").transform.GetChild(0).GetComponent<TextMesh>();
        _hiscore = GameObject.Find("HiScore").transform.GetChild(0).GetComponent<TextMesh>();
        _streak = GameObject.Find("Streak").transform.GetChild(0).GetComponent<TextMesh>();        
        _streakRenderer = _streak.transform.root.GetComponent<MeshRenderer>();
        _streakMaterial = _streakRenderer.material;
        _system = GameObject.Find("ParticularSystem").GetComponent<ParticleSystem>();
        _system.Stop();
        GameObject et = GameObject.Find("Piece");
        _board = GameObject.Find("Board");
        _piece = Instantiate(et);
        _piece.SetActive(false);
        _piece.transform.position = et.transform.position;
        
        _boardForm = _board.transform;
        _systemBoard = _boardForm.GetComponentInChildren<ParticleSystem>();

        _bigText.text = _bigTextSub.text = null;
        _hiscore.text = "0";
        _toplevel.text = "1";

        _checkPointLevel = (int)((float)_maxLevels * 0.1f);

        _chanimation.Setup();

        SetupBoard(size: 3, reset: false);
    }

    void SetupBoard(int size = 5, bool reset = true)
    {
        if(size <= 0 || size > 5){ return; }

        Debug.Log("SETTING UP BOARD");

        CleanupBoard();

        _activePiecesList.Clear();
        _deathPiecesList.Clear();
        _activePieces = _streakCount = _counter = 0;
        _streak.text = _streakCount.ToString();
        _scoreTimer = 0f;
        _boardForm.transform.eulerAngles = Vector3.zero;
        _boardForm.localScale = Vector3.one;

        if(streakRoutine != null && _streaking)
        {
            StopCoroutine(streakRoutine);
            _streaking = false;
            _streakMaterial.color = Color.white;
        }

        if(reset)
        {
            _resetButton.SetActive(false);
            _boardForm.position = Vector3.zero;
            _bigTextForm.position = new Vector3(0f, -5.5f, 0f);
            _chanimation.MagicNumber = 0;
            _camerae.backgroundColor = new Color32(0xCC, 0xCC, 0xCC, 0xFF);
            _gameOver = false;
            _currentScore = _streakCount = 0;
            _currentLevel = _currentLevel >= _checkPointLevel ? _checkPointLevel : 1;
            _bigText.text = _bigTextSub.text = null;

            StopAllCoroutines();            
        }

        _score.text = _currentScore.ToString();
        _level.text = _currentLevel.ToString();

        float theSize = (size * size);
        float weight = theSize * 1.618f;
        float theCalculation = ((float)(_currentLevel < weight ? weight : _currentLevel) / (float)_maxLevels);

        Debug.Log("<b>The calculation is:</b> " + theCalculation + " | " + ((float)_currentLevel / (float)_maxLevels) + " != " + (weight / (float)_maxLevels) + " | " + weight);

        for(int c = 0; c < size; ++c)
        {
            for(int e = 0; e < size; ++e)
            {
                GameObject et = Instantiate(_piece);
                et.name = "Piece" + c + "-" + e;
                et.SetActive(true);

                Transform er = et.transform;
                er.parent = _board.transform;
                Vector3 position = Vector3.zero;
                position.x = ((size + 0.2f) * ((float)c / (float)size)) + (Mathf.Abs(er.localPosition.x - 0.6f) * (1f - ((float)size) / 5f));
                position.y = -((size + 0.2f) * ((float)e / (float)size)) - (Mathf.Abs(er.localPosition.y + 0.6f) * (1f - ((float)size) / 5f));
                er.localPosition += position;

                float eger = 1f - Random.Range((float)_deathPiecesList.Count / (float)theSize, 1f);
                //Debug.Log(eger + " | death: " + _deathPiecesList.Count + " | size: " + theSize);

                if(eger >= theCalculation && _deathPiecesList.Count < theSize)
                {
                    er.name = "Death-" + c.ToString() + "-" + e.ToString();
                    _deathPiecesList.Add(et);

                    eger = Random.Range(0f, 1f);

                    if(_currentLevel >= (int)((float)_maxLevels * 0.05f) && _deathPiecesList.Count >= 3 && eger <= 0.333f)
                    { 
                        TextMesh text = et.transform.GetComponentInChildren<TextMesh>();

                        if(text)
                        {
                            text.text = ((int)Random.Range(1, theSize)).ToString();
                            text.color = new Color32(0xCC, 0x33, 0x33, 0xFF);
                        }

                        er.GetChild(0).GetComponent<MeshRenderer>().enabled = false;  
                        er.GetComponentInChildren<Togglinger>().enabled = false;              
                        
                    }
                    else
                    {
                        er.GetChild(0).gameObject.SetActive(false);
                    }
                }
                else
                {
                    ++_activePieces;
                    _activePiecesList.Add(et);
                }
            }
        }

        if(_activePieces == 0)
        {            
            GameObject et = _boardForm.GetChild((int)Random.Range(0, _boardForm.childCount - 1)).gameObject;
            Debug.Log("Found: " + et.name);
            et = et.transform.GetChild(0).gameObject;
            et.SetActive(true);
            Debug.Log("Activated " + et.name + ": " + et.activeSelf);
            et.GetComponent<MeshRenderer>().material = StartMaterial;
            ++_activePieces;

            TextMesh text = et.transform.GetComponentInChildren<TextMesh>();

            if(text)
            {
                text.text = _activePieces.ToString();
            }
            else
            {
                Debug.LogError("FOUND NOT THE TEXT FOR " + et.name);
            }

            et = et.transform.parent.gameObject;
            et.name = _activePieces.ToString();
            _deathPiecesList.Remove(et);
            _activePiecesList.Add(et);

            Debug.Log("FIXED? " + et.name);
        }
        else
        {
            IEnumerable<int> numbers = Enumerable.Range(1, _activePiecesList.Count);
            IOrderedEnumerable<int> shuffle = numbers.OrderBy(a => _random.NextDouble());

            List<int> egers = shuffle.ToList();

            for(int c = 0; c < _activePiecesList.Count; ++c)
            {
                int eger = egers[c];

                GameObject et = _activePiecesList[c];

                et.name = eger.ToString();
                        
                TextMesh text = et.GetComponentInChildren<TextMesh>();
                text.text = eger == 3 ? ":" + eger.ToString() : eger.ToString();

                if(eger == 1)
                {
                    //text.color = StartMaterial.color;
                    et.transform.GetComponent<MeshRenderer>().material = StartMaterial;
                }
                
                Togglinger togglinger = et.GetComponentInChildren<Togglinger>();
                togglinger.enabled = _currentLevel >= (int)((float)_maxLevels * 0.03f) && Random.Range(0f, 1f) <= 0.25;


                //if(eger == _activePiecesList.Count)
                {
                    //text.color = EndMaterial.color;
                }
            }
        }
    }

    void CleanupBoard()
    {
        if(_boardForm.childCount > 0)
        {
            for(int c = 0; c < _boardForm.childCount; ++c)
            {
                Destroy(_boardForm.GetChild(c).gameObject);
            }
        }
    }

    void Inputs()
    {
        if(_cycling || _rotating || _flipping){ return; }

        _cast = Camera.main.ScreenPointToRay(Input.mousePosition);
            
        _entered = Physics.Raycast(_cast, out _me);

        if (_entered)
        {
            
            if(_colliderName != _me.collider.name)
            {
                if(!_enterSetup)
                {
                    _colliderName = _me.collider.name;
                    Debug.Log(_colliderName);
                    SwitchMaterial(_me.transform);
                    _enterSetup= true;
                }
                else
                {
                    SwitchMaterial(_me.transform, false);
                    _enterSetup = false;
                }
            }            

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                switch(_me.collider.name)
                {
                    case "Reset":
                        ResetAction();
                        break;
                    default:
                        ClickLogic(_me.transform);
                        break;
                }
            }                  
        }
        else
        if(_enterSetup)
        {
            SwitchMaterial(_me.transform, false);
            _colliderName = null;
            _enterSetup = false;
        }   
    }

    void ResetAction()
    {
        _boardSize = _currentLevel >= _checkPointLevel ? _boardSize : 3;
        Debug.Log("<b>CHECKPOINTED = " + _checkPointed + " | </b>" + _boardSize);
        SetupBoard(size: _boardSize);
    }

    void ClickLogic(Transform er)
    {
        _chanimation.IdleSwitch = 0f;

        if(!_gameOver)
        {
            bool death = er.name.Contains("Death");

            if(!death)
            {
                int eger = 0;
                bool parsed = int.TryParse(er.name, out eger);
                
                if(parsed)
                {
                    death = eger > _counter + 1;
                    Debug.Log(eger + " = " + death + " | " + (_counter + 1));
                }
            }

            if(death)
            {
                Debug.Log("<color=red>YOU DIED</color>");
                er.GetChild(0).gameObject.SetActive(false);
                er.GetChild(1).gameObject.SetActive(true);
                
                GameOver();
            }
            else
            {
                Togglinger togglinger = er.GetComponentInChildren<Togglinger>();
                togglinger.enabled = false;

                TextMesh en =  er.GetComponentInChildren<TextMesh>();

                if(!en.transform.GetComponent<MeshRenderer>().enabled)
                {
                    Debug.Log("<color=red>YOU DIED</color>");
                    er.GetChild(1).gameObject.SetActive(true);
                    
                    GameOver();

                    return;
                }
                else
                {
                    --_activePieces;
                    ++_counter;
                    er.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                    er.GetComponentInChildren<Collider>().enabled = false;
                    en.color = _goodColour;
                    en.text = "V";
                    float timer = Time.timeSinceLevelLoad - _scoreTimer;
                    Debug.Log("KLYKK'D ON: " + er.name + " | " + _activePieces + " of " + _activePiecesList.Count + " pieces left!\n" + timer);

                    bool streak = _activePiecesList.Count > 0 && timer < 2f;

                    if(streak)
                    {
                        Debug.Log("STREAKING!");
                        
                        ++_streakCount;
                        _currentScore += 10 * (int)(_activePiecesList.Count - _activePieces);
                        streakRoutine = Streaking();
                        StartCoroutine(streakRoutine);

                        animationRoutine = AnimateHer(4);
                        StartCoroutine(animationRoutine);

                        _streak.text = _streakCount.ToString();
                    }
                    else
                    {
                        _streakCount = 0;
                        _currentScore += 10;
                    }

                    _score.text = _currentScore.ToString();

                    _scoreTimer = Time.timeSinceLevelLoad;

                    _system.transform.position = er.position;
                    _system.Play();

                    if(_activePieces == 0)
                    {
                        AdvanceLevel();
                    }
                    else
                    {
                        _activePiecesList.Remove(er.gameObject);

                        if(_currentLevel >= (int)((float)_maxLevels * 0.25f) && Random.Range(0f, 1f) <= 0.25f)
                        {
                            rotateRoutine = RotateBoard();
                            StartCoroutine(rotateRoutine);
                            return;
                        }
                        else
                        if(_currentLevel >= (int)((float)_maxLevels * 0.15f) && Random.Range(0f, 1f) <= 0.25f)
                        {
                            flipRoutine = FlipBoard();
                            StartCoroutine(flipRoutine);
                            return;
                        }
                        else
                        if(_currentLevel >= (int)((float)_maxLevels * 0.07f) && _activePiecesList.Count == 2 && Random.Range(0f, 1f) <= 0.75f)
                        {
                            for(int c = 0; c < _activePiecesList.Count; ++c)
                            {
                                GameObject et = _activePiecesList[c];

                                animationRoutine = AnimateHer(3);
                                StartCoroutine(animationRoutine);

                                TextMesh text = et.GetComponentInChildren<TextMesh>();
                                text.text = c == 0 ? "!" : "?";
                            }
                        }
                        if(_currentLevel >= (int)((float)_maxLevels * 0.1f) && _activePiecesList.Count > 2 && Random.Range(0f, 1f) <= 0.25f)
                        {
                            IEnumerable<int> numbers = Enumerable.Range(_counter + 1, _activePiecesList.Count);
                            IOrderedEnumerable<int> shuffle = numbers.OrderBy(a => _random.NextDouble());

                            List<int> egers = shuffle.ToList();

                            if(animationRoutine != null && _awaiting)
                            {
                                StopCoroutine(animationRoutine);
                                _awaiting = false;
                            }

                            animationRoutine = AnimateHer(3);
                            StartCoroutine(animationRoutine);

                            for(int c = 0; c < _activePiecesList.Count; ++c)
                            {
                                int eger = egers[c];

                                GameObject et = _activePiecesList[c];

                                et.name = eger.ToString();

                                et.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                                        
                                TextMesh text = et.GetComponentInChildren<TextMesh>();
                                text.text = eger == 3 ? ":" + eger.ToString() : eger.ToString();
                            }
                        }
                    }
                }
            }
        }
    }

    void AdvanceLevel()
    {
        if(animationRoutine != null && _awaiting)
        {
            StopCoroutine(animationRoutine);
            _awaiting = false;
            Debug.Log("<b>STOPPED ANIMATING HER AT</b> " + Time.timeSinceLevelLoad);
        }

        //StopAllCoroutines();

        routine = CycleBoard();
        StartCoroutine(routine);
    }

    void GameOver(bool goodend = false)
    {
        _resetButton.SetActive(true);
        
        if(!goodend)
        {
            if(animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                _awaiting = false;
            }

            //StopAllCoroutines();  

            _chanimation.MagicNumber = 2;
            _camerae.backgroundColor = new Color32(0xCC, 0x33, 0x33, 0xFF);

            Togglinger[] togglingers = FindObjectsOfType<Togglinger>();

            for(int c = 0; c < togglingers.Length; ++c)
            {
                togglingers[c].enabled = false;
            }

            for(int c = 0; c < _deathPiecesList.Count; ++c)
            {
                _deathPiecesList[c].transform.GetChild(1).gameObject.SetActive(true);
                _deathPiecesList[c].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            _system.transform.position = _chanimation.transform.position;
            _system.Play();
            _chanimation.MagicNumber = 8;
        }

        if(_currentScore > _hiScore)
        {
            _hiScore = _currentScore;
            _hiscore.text = _hiScore.ToString();
        }

        if(_currentLevel > _topLevel)
        {
            _topLevel = _currentLevel;
            _toplevel.text = _topLevel.ToString();
        }

        _gameOver = true;
    }

    void SwitchMaterial(Transform er, bool enter = true)
    {
        if(enter)
        {
            _objectRenderer = er.GetComponent<MeshRenderer>();
            _objectMaterial = _objectRenderer.material;
            _objectRenderer.material = HiLiteMaterial;
        }
        else 
        if(_objectRenderer)
        {
            _objectRenderer.material = _objectMaterial;
        }
    }

    void Start()
    {
        Setup();
    }

    void Update()
    {
        Inputs();
        _chanimation.AnimationPlayer();
    }
}
