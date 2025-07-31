using UnityEngine;

namespace LifeCraft.Core
{
    public class GridPopulator : MonoBehaviour
    {
        public GameObject gridCellPrefab;
        public Transform gridParent;
        public int rows = 40; // Number of rows in the grid.
        public int columns = 20; // Number of columns in the grid.

        void Start()
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Instantiate(gridCellPrefab, gridParent);
                }
            }
        }
    }
}