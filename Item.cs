[System.Serializable]
public class Item{

    public string name;

    public int plantPrice;
    public int sellPrice;
    public float growthSpeed;
    public int levelReq;

    public int n; // number in stock

    public void Set(string _name, int _plantPrice, int _sellPrice, float _growthSpeed, int _levelReq)
    {
        this.name = _name;
        this.plantPrice = _plantPrice;
        this.sellPrice = _sellPrice;
        this.growthSpeed = _growthSpeed;
        this.levelReq = _levelReq;
    }

    public void UpdateN(int value)
    {
        this.n += value;
    }
};

