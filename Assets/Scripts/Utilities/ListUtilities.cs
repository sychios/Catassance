using System.Collections.Generic;

public class ListUtilities
{

    // Fisher-Yates-Card-Deck Shuffle
    // Weblink: https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html
    public static List<T> ShuffleList<T>(List<T> list){
        System.Random _random = new System.Random ();
 
        T thing;
 
        int n = list.Count;
        for (int index = 0; index < n; ++index)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = index + (int)(_random.NextDouble() * (n - index));
            thing = list[r];
            list[r] = list[index];
            list[index] = thing;
        }
 
        return list;
    }
}
