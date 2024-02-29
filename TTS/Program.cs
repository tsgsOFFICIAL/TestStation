using System;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;

namespace TTS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Basic TTS
            SpeechSynthesizer synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            synth.Speak("Hello, world!");

            // Changing the Voice
            synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            synth.Speak("Hello, I am a female voice!");

            // Changing the Pitch and Rate
            synth.Rate = -2;
            synth.Volume = 100;
            synth.Speak("Hello, I am speaking slower and louder!");

            // Pausing and Resuming Speech
            synth.Speak("Hello, I will pause for 3 seconds now.");
            synth.Pause();
            Thread.Sleep(3000); // wait for 3 seconds
            synth.Resume();
            synth.Speak("I am back!");

            // Saving Speech to a WAV File
            synth.SetOutputToWaveFile("C:\\Users\\mmj\\Desktop\\output.wav");
            synth.Speak("Hello, I am saving my speech to a WAV file!");

            // Setting the Speech Stream
            MemoryStream stream = new MemoryStream();
            synth.SetOutputToWaveStream(stream);
            synth.Speak("Hello, I am being streamed to a memory stream!");
            byte[] speechBytes = stream.GetBuffer();

            // Changing the Voice and Pronunciation
            PromptBuilder builder = new PromptBuilder();
            builder.StartVoice(VoiceGender.Female, VoiceAge.Adult, 1);
            builder.AppendText("Hello, my name is Emily.");
            builder.EndVoice(); // Corrected: End the voice selection
            
            builder.StartVoice(VoiceGender.Female, VoiceAge.Teen, 2);
            builder.AppendText("I am from New York City.");
            builder.EndVoice(); // Corrected: End the voice selection
           
            builder.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Strong });
            builder.AppendText("I really love chocolate!");
            builder.EndStyle(); // Corrected: End the style
           
            builder.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Reduced });
            builder.AppendText("But I'm allergic to it...");
            builder.EndStyle(); // Corrected: End the style

            synth.Speak(builder);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}