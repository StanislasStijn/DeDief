using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OfficeGenerator : MonoBehaviour
{
    public float MinArea = 3;
    [Range(0.0f, 1.0f)]
    public float MaxHallRate = 0.15F;
    public float HallSize = 1;
    public GameObject Corridor;
    public List<RoomType> RoomTypes;

    private Size Size;
    private Area House;
    private double TotalHallArea;
    private List<Area> Chunks, Halls, Blocks, UnreachableAreas, Areas;
    private List<GameObject> Rooms;

    private void Start()
    {
        Size = GetComponent<Size>();
        Generate();
    }

    public void Generate()
    {
        // Remove and reset everyting
        foreach (Transform c in transform) {
            Destroy(c.gameObject);
        }
        Rooms = new List<GameObject>();
        TotalHallArea = 0;
        House = new Area(0, 0, Size.size.x, Size.size.z);
        Chunks = new List<Area>();
        Halls = new List<Area>();
        Blocks = new List<Area>();
        UnreachableAreas = new List<Area>();
        Areas = new List<Area>();

        // Generate office
        ChunksToBlocks();
        BlocksToAreas();
        AddDoors();

        foreach (Area area in Areas)
        {
            PlaceRoom(area);
        }
        foreach (Area hall in Halls)
        {
            PlaceArea(hall, Corridor);
        }
    }

    private void ChunksToBlocks()
    {
        Chunks.Add(House);
        while ((Chunks.Count > 0) && (TotalHallArea / (double) House.GetArea() < MaxHallRate))
        {
            Area chunk = Chunks.Max();
            Chunks.Remove(chunk);

            if (chunk.GetArea() > MinArea)
            {
                (Area chunk_a, Area hall, Area chunk_b) = chunk.SplitThree(HallSize);
                Chunks.Add(chunk_a);
                Chunks.Add(chunk_b);
                Halls.Add(hall);
                TotalHallArea += hall.GetArea();
            }
            else
            {
                Blocks.Add(chunk);
            }
        }
        Blocks.AddRange(Chunks);
        Chunks.Clear();
    }

    private void BlocksToAreas()
    {
        while (Blocks.Count > 0)
        {
            Area block = Blocks.Max();
            Blocks.Remove(block);

            if (WantSplitBlock(block))
            {
                (Area block_a, Area block_b) = block.SplitTwo();
                Blocks.Add(block_a);
                Blocks.Add(block_b);
            }
            else
            {
                UnreachableAreas.Add(block);
            }
        }
    }

    private bool WantSplitBlock(Area block)
    {
        if (block.GetArea() < MinArea)
        {
            return false;
        }
        else
        {
            double chance = Random.Range(0, (float) block.GetArea());
            if (chance < MinArea)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public void AddDoors()
    {
        while (UnreachableAreas.Count > 0)
        {
            Area area = UnreachableAreas[0];
            UnreachableAreas.Remove(area);
            bool connected = false;
            foreach (Area hall in Halls)
            {
                if (hall.IsTouching(area))
                {
                    area.AddDoorTo(hall);
                    connected = true;
                    break;
                }
            }
            if (!connected)
            {
                foreach (Area areaCheck in Areas)
                {
                    if (areaCheck.IsTouching(area))
                    {
                        area.AddDoorTo(areaCheck);
                        connected = true;
                        break;
                    }
                }
            }
            
            if (!connected)
            {
                UnreachableAreas.Add(area);
            } else
            {
                Areas.Add(area);
            }
        }
    }

    public void PlaceRoom(Area area)
    {
        GameObject roomPrefab = getGoodRoom(area);
        PlaceArea(area, roomPrefab);
    }

    public void PlaceArea(Area area, GameObject roomPrefab)
    {
        GameObject room = Instantiate(roomPrefab, transform);
        Size size = room.GetComponent<Size>();
        if (size == null)
        {
            size = room.AddComponent<Size>();
        }
        if (area.Door != null)
        {
            Vector2 offset = new Vector2((float)(area.Left + area.GetWidth() / 2), (float)(area.Top + area.GetLength() / 2));
            Door door = room.AddComponent<Door>();
            door.placeBetween(new Vector2(area.Door.Left, area.Door.Top) - offset, new Vector2(area.Door.Right, area.Door.Bottom) - offset);
        }
        size.size = new Vector3((float)area.GetWidth(), Size.size.y, (float)area.GetLength());
        room.transform.position = new Vector3((float)(area.Left + area.GetWidth() / 2), 0, (float)(area.Top + area.GetLength() / 2));
        Rooms.Add(room); 
    }

    public void OnDestroy()
    {
        foreach (GameObject room in this.Rooms)
        {
            Destroy(room);
        }
    }

    public GameObject getGoodRoom(Area area)
    {
        RoomType item = RoomTypes.OrderBy(x => x.getScore(area)).FirstOrDefault(); ;
        return item.RoomPrefab;
    }
}

[System.Serializable]
public class RoomType
{
    public string Name;
    public float AvarageSize;
    public GameObject RoomPrefab;


    public float getScore(Area area)
    {
        return Mathf.Abs((float)(area.GetArea() - AvarageSize));
    }
}