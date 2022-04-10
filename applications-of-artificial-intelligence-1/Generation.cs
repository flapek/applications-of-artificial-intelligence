namespace applications_of_artificial_intelligence_1;

internal class Generation
{
    public int[] Population { get; private set; }
    public int Mark { get; private set; }

    public Generation(int[] population, int mark)
    {
        Population = population;
        Mark = mark;
    }
    
    public void UpdateMark(int mark)
        => Mark = mark;
};