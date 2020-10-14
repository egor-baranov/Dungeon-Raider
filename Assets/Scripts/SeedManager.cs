using UnityEngine;
using Random = UnityEngine.Random;

public class SeedManager {
    private SeedManager() {
        _originalRandomState = Random.state;
        Seed = (int) System.DateTime.Now.Ticks;
        Random.InitState(Seed);
        Debug.Log($"current generation seed is {Seed}");
    }

    private SeedManager(int val) {
        _originalRandomState = Random.state;
        Seed = val;
        Random.InitState(val);
        Debug.Log($"current generation seed is {Seed}");
    }


    private static Random.State _originalRandomState; 
    public static int Seed { get; private set; }

    private static SeedManager _instance;

    // create new instance of SeedManager or use current
    public static SeedManager Instance => _instance ?? (_instance = new SeedManager());

    // init new SeedManager using existing seed
    public static SeedManager Init() => Instance;
    public static SeedManager Init(int seed) => _instance ?? (_instance = new SeedManager(seed));

    public static void Refresh() {
        Random.state = _originalRandomState;
        Random.InitState(Seed);
    }
}