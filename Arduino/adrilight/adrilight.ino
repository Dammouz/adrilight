#include "FastLED.h"

#define NUM_LEDS  60 //(2 * 26 + 2 * 4)
#define DATA_PIN  42
#define CLOCK_PIN 48
#define BRIGHTNESS 200 // Range is 0..255 with 255 beeing the MAX brightness

// --------------------------------------------------------------------------------------------
// NO CHANGE REQUIRED BELOW THIS LINE
// --------------------------------------------------------------------------------------------

#define UPDATES_PER_SECOND 60
#define TIMEOUT 3000

#define MODE_ANIMATION 0
#define MODE_AMBILIGHT 1
#define MODE_BLACK     2
uint8_t mode = MODE_ANIMATION;

#define RGB_BYTES 3
#define INDEX_COLOR_RED   0
#define INDEX_COLOR_GREEN 1
#define INDEX_COLOR_BLUE  2
uint8_t currentBrightness = BRIGHTNESS;

#define PREAMBLE_LENGTH 10
const byte MESSAGE_PREAMBLE[PREAMBLE_LENGTH] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
const byte MESSAGE_POSTAMBLE[RGB_BYTES] = { 85, 204, 165 };
uint8_t current_preamble_position = 0;

unsigned long last_serial_available = -1L;

CRGB leds[NUM_LEDS * 2];
CRGB ledsTemp[NUM_LEDS];
byte buffer[RGB_BYTES];

// Filler animation attributes
CRGBPalette16 currentPalette = RainbowColors_p;
TBlendType currentBlending = LINEARBLEND;
uint8_t startIndex = 0;

void setup()
{
    Serial.begin(1000000);
    FastLED.clear(true);
    //FastLED.addLeds<WS2812B, LED_DATA_PIN, GRB>(leds, NUM_LEDS);
    FastLED.addLeds<APA102, DATA_PIN, CLOCK_PIN, RGB>(leds, NUM_LEDS * 2);
    FastLED.setBrightness(currentBrightness);
    FastLED.setDither(0);
}

void loop()
{
    switch (mode)
    {
        case MODE_ANIMATION:
            fillLEDsFromPaletteColors();
            break;

        case MODE_AMBILIGHT:
            processIncomingData();
            break;

        case MODE_BLACK:
            showBlack();
            break;
    }
}

void processIncomingData()
{
    if (waitForPreamble(TIMEOUT))
    {
        for (int ledNum = 0; ledNum < NUM_LEDS + 1; ledNum++)
        {
            // We always have to read the R, G and B bytes
            // If it is less, we ignore this frame and wait for the next preamble
            if (Serial.readBytes((char *)buffer, RGB_BYTES) < RGB_BYTES)
            {
                return;
            }

            if (ledNum < NUM_LEDS)
            {
                byte red = buffer[INDEX_COLOR_RED];
                byte green = buffer[INDEX_COLOR_GREEN];
                byte blue = buffer[INDEX_COLOR_BLUE];
                ledsTemp[ledNum] = CRGB(red, green, blue);
            }
            else if (ledNum == NUM_LEDS)
            {
                // This must be the "postamble"
                // This last "color" is actually a closing preamble
                // If the postamble does not match the expected values, the colors will not be shown
                if (buffer[INDEX_COLOR_RED] == MESSAGE_POSTAMBLE[INDEX_COLOR_RED]
                     && buffer[INDEX_COLOR_GREEN] == MESSAGE_POSTAMBLE[INDEX_COLOR_GREEN]
                     && buffer[INDEX_COLOR_BLUE] == MESSAGE_POSTAMBLE[INDEX_COLOR_BLUE])
                {
                    // The preamble is correct, update the leds!

                    // TODO Can we flip the used buffer instead of copying the data?
                    for (int ledNum = 0; ledNum < NUM_LEDS; ledNum++)
                    {
                        leds[ledNum * 2] = ledsTemp[ledNum];
                    }

                    if (currentBrightness < BRIGHTNESS)
                    {
                        currentBrightness++;
                        FastLED.setBrightness(currentBrightness);
                    }

                    // Send LED data to actual LEDs
                    FastLED.show();
                }
            }
        }
    }
    else
    {
        // If we get here, there must have been data before (so the user already knows, it works!)
        // Simply go to black!
        mode = MODE_BLACK;
    }
}

bool waitForPreamble(int timeout)
{
    last_serial_available = millis();
    current_preamble_position = 0;
    while (current_preamble_position < PREAMBLE_LENGTH)
    {
        if (Serial.available() > 0)
        {
            last_serial_available = millis();

            if (Serial.read() == MESSAGE_PREAMBLE[current_preamble_position])
            {
                current_preamble_position++;
            }
            else
            {
                current_preamble_position = 0;
            }
        }

        if (millis() - last_serial_available > timeout)
        {
            return false;
        }
    }

    return true;
}

void fillLEDsFromPaletteColors()
{
    startIndex++;

    uint8_t colorIndex = startIndex;
    for (int i = 0; i < NUM_LEDS; i++)
    {
        leds[i * 2] = ColorFromPalette(currentPalette, colorIndex, BRIGHTNESS, currentBlending);
        colorIndex += RGB_BYTES;
    }

    switchToAmbilightMode();
}

void showBlack()
{
    if (currentBrightness > 0)
    {
        currentBrightness--;
        FastLED.setBrightness(currentBrightness);
    }

    switchToAmbilightMode();
}

void switchToAmbilightMode()
{
    FastLED.delay(1000 / UPDATES_PER_SECOND);

    if (Serial.available() > 0)
    {
        mode = MODE_AMBILIGHT;
    }
}
