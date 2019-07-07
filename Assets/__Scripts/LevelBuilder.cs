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

        public int LevelSize;
        public Texture2D mapLiberty;
        public GameObject end;
        public int blocSize;
        

        // Start is called before the first frame update
        void Start()
        {
            _tilemap = GetComponent<Tilemap>();
            foreach(var t in GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name.Equals("Start")) _start = t.gameObject;
            }
            mapLiberty = new Texture2D(LevelSize * blocSize, 20 * blocSize);
        }


        void PlaceBloc(int i, int j, GameObject bloc)
        {
            var liberty = bloc.GetComponent<MetaData>()._liberty;
            liberty.Resize(Mathf.FloorToInt(liberty.width * 0.2f), Mathf.FloorToInt(liberty.height * 0.2f));
            for (int k = 0; k < liberty.width; k++)
            {
                for (int l = 0; l < liberty.height; l++)
                {
                    var col = liberty.GetPixel(i, j);
                    col += mapLiberty.GetPixel(i * blocSize + k, j * blocSize + l);
                    mapLiberty.SetPixel(i * blocSize + k, j * blocSize + l, col);
                }
            }

        }

        
    }
}
