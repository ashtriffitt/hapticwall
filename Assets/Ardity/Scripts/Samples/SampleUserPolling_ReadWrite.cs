/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO.Ports;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class SampleUserPolling_ReadWrite : MonoBehaviour
{
    public SerialController serialController;

    Encoding utf8;
    Encoding unicode;

    // Initialization
    void Start()

    {


        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();

        Debug.Log("Press A or Z to execute some actions");

        unicode = Encoding.Unicode;
        utf8 = Encoding.UTF8;

    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------

        // If you press one of these keys send it to the serial device. A
        // sample serial device that accepts this input is given in the README.
        if (Input.GetKeyDown(KeyCode.A)) {
            Debug.Log("Sending A");
            ushort number = 5000;

            MoveMotor(number);
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            Debug.Log("Sending Z");

            ushort number = 8;

            MoveMotor(number);

        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Sending SPACE");
            ushort number = 1700;

            MoveMotor(number);
        }

        if (Input.GetKeyDown(KeyCode.P)) {

            Debug.Log("Sending P");
            int number = -1500;

            MoveMotor(number);
        }



        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        string message = serialController.ReadSerialMessage();

        if (message == null)
            return;

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
            Debug.Log("Connection established");
        else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
            Debug.Log("Connection attempt failed or disconnection detected");
        else
            Debug.Log("Message arrived: " + message);
    }


    // Moves motor by given amt
    public void MoveMotor(int number)
    {
        byte lower = (byte)(number & 0xFF);
        byte upper = (byte)((number >> 8) & 0xFF);

        char[] chars = new char[] { (char)lower, (char)upper }; // Converts amt to 2 bytes, converts bytes to chars, sends to arduino;

        Debug.Log("Lower: " + lower);
        Debug.Log("Upper: " + upper);

        serialController.serialThread.serialPort.Write(chars, 0, 2);
    }
}
