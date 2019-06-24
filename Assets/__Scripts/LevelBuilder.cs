using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace __Scripts
{
    [RequireComponent(typeof(Tilemap))]
    public class LevelBuilder : MonoBehaviour
    {
        private Tilemap _tilemap;
        
        private GameObject _start;
        [Range(1,10)] public int levelLength = 1;

        public GameObject end;

        public GameObject[] blocs;
        
        private bool _levelMade = false;
        private readonly List<float> _crList = new List<float>();
        private readonly List<GameObject> _levelBlocs = new List<GameObject>();


        // Start is called before the first frame update
        void Start()
        {
            _tilemap = GetComponent<Tilemap>();
            foreach(var t in GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name.Equals("Start")) _start = t.gameObject;
            }
            if (!_levelMade)
            {
                _crList.Add(0f);
                while (_levelBlocs.Count < levelLength)
                {
                    _levelBlocs.Add(SelectBlocToCurve());
                    _crList.Add(_levelBlocs[_levelBlocs.Count - 1].GetComponent<MetaData>().ChallengeRating);
                    Debug.Log("CR - bloc : " + _crList[_crList.Count-1] + " // CR - cumulatif : " + CurrentChallengeRating());//aff0
                }
                _levelBlocs.Add(end);
                InstantiateLevel();
                CurveChallenge();
                _levelMade = true;
            }
        }

        private void CurveChallenge()
        {
            //ajouter des pièges en cumulatif jusqu'à la fin
            for (int i = 0; i < _levelBlocs.Count; i++)
            {
                for (int j = 0; j < (i / ((_levelBlocs.Count / 4))); j++)
                {
                    var o = AddTrap(_levelBlocs[i].GetComponent<MetaData>().Additions);
                    switch (o.tile.name)
                    {
                        case "SpikesUp":
                            foreach (var t in _levelBlocs[i].GetComponentsInChildren<Transform>())
                            {
                                //split and randomize //aff0
                                if (!t.gameObject.name.Contains("Hill"))
                                {
                                    var y = t.position;
                                    y.y += _tilemap.tileAnchor.y * 2;
                                    Instantiate(o.tile, y, t.rotation, t);
                                    _crList[i + 1] += o.CR;
                                    break;
                                }
                            }
                            break;
                        case "Mace":
                            foreach (var t in _levelBlocs[i].GetComponentsInChildren<Transform>())
                            {
                                    var y = t.position;
                                    y.y += _tilemap.tileAnchor.y * 2 * Random.Range(2f,4f);
                                    Instantiate(o.tile, y, t.rotation, t);
                                    _crList[i + 1] += o.CR;
                                    break;

                            }
                            break;
                        case "Saw":
                            foreach (var t in _levelBlocs[i].GetComponentsInChildren<Transform>())
                            {
                                var y = t.position;
                                y.y += _tilemap.tileAnchor.y * 2 * Random.Range(2f,4f);
                                Instantiate(o.tile, y, t.rotation, t);
                                _crList[i + 1] += o.CR;
                                break;

                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            
        }

        private MetaAugmentingTile AddTrap(MetaAugmentingTile[] objects)
        {
            var r = Random.Range(0, objects.Length);
            return objects[r];
        }
        private void InstantiateLevel()
        {
            var cursor = _start;
            for (int i = 0; i < _levelBlocs.Count; i++)
            {
                var t = cursor.transform.position;
                var diff = new Vector3(0,0,0);
                if (cursor.GetComponent<MetaData>())
                {
                    t += cursor.GetComponent<MetaData>().junctionEst.transform.localPosition;
                    
                }
                if (_levelBlocs[i].GetComponent<MetaData>())
                {
                    diff.x = -_levelBlocs[i].GetComponent<MetaData>().junctionOuest.transform.localPosition.x;
                    diff.y = -_levelBlocs[i].GetComponent<MetaData>().junctionOuest.transform.localPosition.y;
                }

                _levelBlocs[i].name = "bloc"+i; //aff0
                t.x += _tilemap.tileAnchor.x * 2 + diff.x;
                t.y += diff.y;

                cursor = Instantiate(_levelBlocs[i], t, cursor.transform.rotation,transform);
                _levelBlocs[i] = cursor;

            }
        }
        
        private GameObject SelectBlocToCurve()
        {
           
                var shuffle = Random.Range(0, blocs.Length);
                return blocs[shuffle];
           
        }

        private float CurrentChallengeRating()
        {
            var s = 0f;
            foreach (var cr in _crList)
            {
                s += cr;
            }

            return s;
        }
        

        private GameObject AddItem(char direction, GameObject o, GameObject cursor)
        {
            Debug.Log("AddItem : " + direction);
            var t = cursor.transform.position;
            switch (direction)
            {
                case 'N':
                    t.y += _tilemap.tileAnchor.y * 2;
                    break;
                case 'E':
                    t.x += _tilemap.tileAnchor.x * 2;
                    break;
                case 'S':
                    t.y -= _tilemap.tileAnchor.y * 2;
                    break;
                case 'O':
                    t.x -= _tilemap.tileAnchor.x * 2;
                    break;
                default:
                    break;
            }
            
            var i = Instantiate(o,transform);
            
            i.transform.position = t;
            
            return i;
        }
    }
}
