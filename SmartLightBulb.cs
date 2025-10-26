using UnityEngine;

/// <summary>
/// This class demonstrates how to receive and react to messages sent through the ChannelBus.
/// The SmartLightBulb acts as a listener, waiting for commands broadcasted on a shared numeric channel.
/// When it receives a matching message, it performs the appropriate action.
/// </summary>
public class SmartLightBulb : MonoBehaviour
{
    // The channel number this light bulb listens to.
    // It must match the sender’s channel (for example, the SwitchController) to receive messages.
    private const int SWITCH_CHANNEL = 345;

    // Possible commands that this light bulb can understand.
    // You can define any number of custom string commands for your own systems.
    private const string TURN_COMMAND = "Turn on";
    private const string TURN_OFF_COMMAND = "Turn off";
    private const string TOGGLE_COMMAND = "TOGGLE";

    private void OnEnable()
    {
        // Ensure we subscribe to the channel when the object is enabled.
        ChannelBus.Listen(SWITCH_CHANNEL, OnCommandReceived);
    }

    private void OnDisable()
    {
        // Clean up the subscription when the object is disabled to prevent memory leaks.
        ChannelBus.ListenRemove(SWITCH_CHANNEL, OnCommandReceived);
    }

    /// <summary>
    /// This method is automatically called when a message arrives on the subscribed channel.
    /// The message is passed as a string, which can represent any type of command or data.
    /// </summary>
    private void OnCommandReceived(string command)
    {
        switch (command)
        {
            case TURN_COMMAND:
                TurnOn();
                break;
            case TURN_OFF_COMMAND:
                TurnOff();
                break;
            case TOGGLE_COMMAND:
                Toggle();
                break;
            default:
                // Ignore any unknown or unsupported commands.
                break;
        }
    }

    /// <summary>
    /// Example logic for turning on the light bulb.
    /// Replace with your actual visual or functional behavior (like enabling a Light component).
    /// </summary>
    private void TurnOn()
    {
        // Logic to turn on the light bulb
    }

    /// <summary>
    /// Example logic for turning off the light bulb.
    /// </summary>
    private void TurnOff()
    {
        // Logic to turn off the light bulb
    }

    /// <summary>
    /// Example logic for toggling the light bulb’s state.
    /// This demonstrates that one message can trigger an action without direct references.
    /// </summary>
    private void Toggle()
    {
        Debug.Log("Light toggled");
    }
}
