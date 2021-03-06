﻿namespace ShiftingDungeon.Dungeon
{
    using System.Collections.Generic;
    using UnityEngine;
    using Character.Enemies;
    using ProcGen.RoomGen;
    using ObjectPooling;
    using RoomParts;
    using Util;

    public class Room : MonoBehaviour
    {
        [SerializeField]
        private int row = 0;
        [SerializeField]
        private int col = 0;
        [SerializeField]
        private bool isGenerated = false;

        [SerializeField]
        private Room up = null;
        [SerializeField]
        private Room down = null;
        [SerializeField]
        private Room left = null;
        [SerializeField]
        private Room right = null;

        [SerializeField]
        internal Door upperDoor = null;
        [SerializeField]
        internal Door lowerDoor = null;
        [SerializeField]
        internal Door leftDoor = null;
        [SerializeField]
        internal Door rightDoor = null;

        /// <summary> The index of this room in the map. </summary>
        public int Index { get; private set; }
        /// <summary> The map row this room is on. </summary>
        public int Row { get { return this.row; } internal set { this.row = value; } }
        /// <summary> The map column this room is on. </summary>
        public int Col { get { return this.col; } internal set { this.col = value; } }
        /// <summary> True if this room is being proceedurally generated. </summary>
        public bool IsGenerated { get { return this.isGenerated; } internal set { this.isGenerated = value; } }
        /// <summary> The room above this one if it exists. </summary>
        public Room Up { get { return this.up; } internal set { this.up = value; } }
        /// <summary> The room below this one if it exists. </summary>
        public Room Down { get { return this.down; } internal set { this.down = value; } }
        /// <summary> The room to the left of this one if it exists. </summary>
        public Room Left { get { return this.left; } internal set { this.left = value; } }
        /// <summary> The room to the right of this one if it exists. </summary>
        public Room Right { get { return this.right; } internal set { this.right = value; } }
        /// <summary> The procedurally generated room. </summary>
        public int[,] Grid { get; private set; }

        private List<GameObject> doors;
        private List<GameObject> walls;
        private List<GameObject> floors;
        private List<GameObject> holes;
        private List<Enemy> enemies;

        /// <summary> Initializes this Room. </summary>
        internal void Init(int index)
        {
            this.Index = index;

            if (!this.IsGenerated)
            {
                if (!CheckDoors())
                    return;

                this.upperDoor.Init(this, this.Up);
                this.lowerDoor.Init(this, this.Down);
                this.leftDoor.Init(this, this.Left);
                this.rightDoor.Init(this, this.Right);
                this.Grid = null;
            }
            else
            {
                CellularAutomata ca = new CellularAutomata(15);
                ca.GeneratePattern(30);
                this.Grid = ca.GetPattern();
            }

            this.doors = new List<GameObject>();
            this.walls = new List<GameObject>();
            this.floors = new List<GameObject>();
            this.holes = new List<GameObject>();
            this.enemies = new List<Enemy>();
            Deactivate();
        }

        public void Activate()
        {
            if (this.IsGenerated)
            {
                AllocateObjects();
            }

            this.gameObject.SetActive(true);
            this.upperDoor.Init(this, this.Up);
            this.lowerDoor.Init(this, this.Down);
            this.leftDoor.Init(this, this.Left);
            this.rightDoor.Init(this, this.Right);
        }

        public void Deactivate()
        {
            foreach (GameObject door in this.doors)
                RoomPool.Instance.ReturnDoor(door);
            this.doors.Clear();

            foreach (GameObject wall in this.walls)
                RoomPool.Instance.ReturnWall(wall);
            this.walls.Clear();

            foreach (GameObject floor in this.floors)
                RoomPool.Instance.ReturnFloor(floor);
            this.floors.Clear();

            foreach (GameObject hole in this.holes)
                RoomPool.Instance.ReturnHole(hole);
            this.holes.Clear();

            foreach (Enemy e in this.enemies)
                EnemyPool.Instance.ReturnEnemy(e.Type, e.gameObject);
            this.enemies.Clear();

            this.gameObject.SetActive(false);
        }
        
        private bool CheckDoors()
        {
            if (this.upperDoor == null)
            {
                Debug.LogError("Room " + this.name + " missing upper door");
                return false;
            }
            if (this.lowerDoor == null)
            {
                Debug.LogError("Room " + this.name + " missing lower door");
                return false;
            }
            if (this.leftDoor == null)
            {
                Debug.LogError("Room " + this.name + " missing left door");
                return false;
            }
            if (this.rightDoor == null)
            {
                Debug.LogError("Room " + this.name + " missing right door");
                return false;
            }

            return true;
        }

        private void AllocateObjects()
        {
            AllocateDoors();
            AllocateWalls();
            AllocateFloorsAndHoles();
            AllocateEnemies();
        }

        private void AllocateDoors()
        {
            this.upperDoor = RoomPool.Instance.GetDoor().GetComponent<Door>();
            this.lowerDoor = RoomPool.Instance.GetDoor().GetComponent<Door>();
            this.leftDoor = RoomPool.Instance.GetDoor().GetComponent<Door>();
            this.rightDoor = RoomPool.Instance.GetDoor().GetComponent<Door>();

            this.upperDoor.transform.position = new Vector3(0, 5, Constants.ROOM_PART_Z_DEPTH);
            this.upperDoor.transform.rotation = Quaternion.identity;
            this.upperDoor.transform.localScale = Vector3.one;

            this.lowerDoor.transform.position = new Vector3(0, -11, Constants.ROOM_PART_Z_DEPTH);
            this.lowerDoor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            this.lowerDoor.transform.localScale = Vector3.one;

            this.leftDoor.transform.position = new Vector3(-8, -3, Constants.ROOM_PART_Z_DEPTH);
            this.leftDoor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            this.leftDoor.transform.localScale = Vector3.one;

            this.rightDoor.transform.position = new Vector3(8, -3, Constants.ROOM_PART_Z_DEPTH);
            this.rightDoor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
            this.rightDoor.transform.localScale = Vector3.one;

            this.doors.Add(this.upperDoor.gameObject);
            this.doors.Add(this.lowerDoor.gameObject);
            this.doors.Add(this.leftDoor.gameObject);
            this.doors.Add(this.rightDoor.gameObject);
        }

        private void AllocateWalls()
        {
            GameObject wall = RoomPool.Instance.GetWall();
            wall.transform.position = new Vector3(0, 0, Constants.ROOM_PART_Z_DEPTH);
            wall.transform.rotation = Quaternion.identity;
            wall.transform.localScale = Vector3.one;
            this.walls.Add(wall);
        }

        private void AllocateFloorsAndHoles()
        {
            for(int r = 0; r < this.Grid.GetLength(0); r++)
            {
                for (int c = 0; c < this.Grid.GetLength(1); c++)
                {
                    if (this.Grid[r, c] == 1)
                    {
                        GameObject floor = RoomPool.Instance.GetFloor();
                        floor.transform.position = new Vector3(-7 + r, 4 - c, Constants.ROOM_PART_Z_DEPTH);
                        floor.transform.rotation = Quaternion.identity;
                        floor.transform.localScale = Vector3.one;
                        this.floors.Add(floor);
                    }
                    else
                    {
                        GameObject hole = RoomPool.Instance.GetHole();
                        hole.transform.position = new Vector3(-7 + r, 4 - c, Constants.ROOM_PART_Z_DEPTH);
                        hole.transform.rotation = Quaternion.identity;
                        hole.transform.localScale = Vector3.one;
                        this.holes.Add(hole);
                    }
                }
            }
        }

        private void AllocateEnemies()
        {
            List<Vector2> positions = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                bool placed = false;
                int tries = 0;
                while (!placed && tries < 100)
                {
                    int r = Random.Range(1, 15);
                    int c = Random.Range(1, 15);
                    Vector2 position = new Vector2(-7 + r, 4 - c);
                    if (this.Grid[r, c] == 1 && !positions.Contains(position))
                    {
                        GameObject enemy = EnemyPool.Instance.GetEnemy(Enums.EnemyTypes.Basic);
                        enemy.transform.position = position;
                        enemy.transform.rotation = Quaternion.identity;
                        this.enemies.Add(enemy.GetComponent<Enemy>());
                        positions.Add(position);
                        placed = true;
                    }

                    tries++;
                }
            }
        }
    }
}
