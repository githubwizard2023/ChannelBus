using UnityEngine;

/// <summary>
/// This class demonstrates how to receive and react to messages sent through the ChannelBus.
/// The SmartLightBulb acts as a listener, waiting for commands broadcasted on a shared numeric channel.
/// When it receives a matching message, it performs the appropriate action.
/// </summary>
public class SmartLightBulb : MonoBehaviour
{
 
    private void OnEnable()
    {
        // Ensure we subscribe to the channel when the object is enabled.
        ChannelBus.Listen(Channels.COMMUNICATION_CHANNEL, OnCommandReceived);
    }

    private void OnDisable()
    {
        // Clean up the subscription when the object is disabled to prevent memory leaks.
        ChannelBus.ListenRemove(Channels.COMMUNICATION_CHANNEL  , OnCommandReceived);
    }

    /// <summary>
    /// This method is automatically called when a message arrives on the subscribed channel.
    /// The message is passed as a string, which can represent any type of command or data.
    /// </summary>
    private void OnCommandReceived(string command)
    {
        switch (command)
        {
            case Commands.TURN_ON:
                TurnOn();
                break;
            case Commands.TURN_OFF:
                TurnOff();
                break;
            case Commands.TOGGLE:
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
