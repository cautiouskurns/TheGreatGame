public class ResourcePackage
{
    public int gold;
    public int food;
    public int production;
    
    public ResourcePackage(int gold = 0, int food = 0, int production = 0)
    {
        this.gold = gold;
        this.food = food;
        this.production = production;
    }
    
    // Add another package to this one
    public void Add(ResourcePackage other)
    {
        gold += other.gold;
        food += other.food;
        production += other.production;
    }
}