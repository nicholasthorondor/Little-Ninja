using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    [SerializeField] GameObject player;
    [SerializeField] LayerMask whatIsGround; // Allows placing of all layers except for the player layer in the inspector to signify that everything but the player is ground.
    [SerializeField] LayerMask whatIsWall; // Allows placing of wall layers in the inspector to signify what is a wall.
    [SerializeField] LayerMask whatIsObject; // Allows placing of object layers in the inspector to signify what clasifies as an object.

    // Getters + Setters
    public GameObject Player {
        get {
            return player;
        }
    }

    public LayerMask WhatIsGround {
        get {
            return whatIsGround;
        }
    }

    public LayerMask WhatIsWall {
        get {
            return whatIsWall;
        }
    }

    public LayerMask WhatIsObject {
        get {
            return whatIsObject;
        }
    }

    void Awake () {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (gameObject);

        Assert.IsNotNull (player);
    }
}
