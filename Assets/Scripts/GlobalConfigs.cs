public static class GlobalConfigs
{
    // **********************************************************************
    // CONTROLLER
    // **********************************************************************
    public const string CONTROLLER_PUNCH = "Punch";
    public const string CONTROLLER_SPECIAL = "Special";
    public const string CONTROLLER_JUMP = "Jump";
    public const string CONTROLLER_RANGED = "Ranged";
    public const string CONTROLLER_LEFT_JOYSTICK_X = "LeftJoystickX";
    public const string CONTROLLER_LEFT_JOYSTICK_Y = "LeftJoystickY";
    public const string CONTROLLER_RIGHT_JOYSTICK_X = "RightJoystickX";
    public const string CONTROLLER_RIGHT_JOYSTICK_Y = "RightJoystickY";
    public const string CONTROLLER_HORIZONTAL = "Horizontal";
    public const string CONTROLLER_VERTICAL = "Vertical";

    public const float CONTROLLER_JOYSTICK_THRESHOLD = 0.45f;
    public const float CONTROLLER_KEYBOARD_THRESHOLD = 0.05f;

    public const string CONTROLLER_TYPE_XBOX360 = "Xbox 360";
    public const string CONTROLLER_TYPE_KEYBOARD = "Keyboard";

    // **********************************************************************
    // TAGS
    // **********************************************************************
    public const string TAG_PLAYER = "PlayerTag";
    public const string TAG_SPIRIT = "SpiritTag";

    // **********************************************************************
    // SORTING LAYER
    // **********************************************************************
    public const string LAYER_SORTING_BACKGROUND = "Background";
    public const string LAYER_SORTING_SPIRIT = "Spirit";
    public const string LAYER_SORTING_PLAYER = "Player";
    public const string LAYER_SORTING_FOREGROUND = "Foreground";

    // **********************************************************************
    // COLLISION LAYER
    // **********************************************************************
    public const string LAYER_COLLISION_BACKGROUND = "Background";
    public const string LAYER_COLLISION_GROUND = "Ground";
    public const string LAYER_COLLISION_PLAYER = "Player";
    public const string LAYER_COLLISION_ENEMY = "Enemy";

    // **********************************************************************
    // ANIMATION
    // **********************************************************************

    // player
    public const string ANIMATION_PLAYER_RUNNING = "Running";

    // enemy

    // **********************************************************************
    // MISC
    // **********************************************************************
    public const float ENTITY_RUN_VELOCITY_EPSILON = 0.05f;
    public const int ENTITY_MAX_HEALTH = 20;
}
