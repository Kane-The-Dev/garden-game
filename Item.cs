[System.Serializable]
public class Item{

    public int ID;
    public string name;
    public int plantPrice;
    public int sellPrice;
    public float growthSpeed;
    public int levelReq;
    public string type;

    public int n; // number in stock

    public void Set(int _ID, string _name, int _plantPrice, 
        int _sellPrice, float _growthSpeed, int _levelReq, string _type)
    {
        this.ID = _ID;
        this.name = _name;
        this.plantPrice = _plantPrice;
        this.sellPrice = _sellPrice;
        this.growthSpeed = _growthSpeed;
        this.levelReq = _levelReq;
        this.type = _type;
    }

    public void UpdateN(int value)
    {
        this.n += value;
    }
};

