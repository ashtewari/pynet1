using Python.Runtime;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        // NOTE: set this based on your python install. this will resolve from
        // your PATH environment variable as well.
        Runtime.PythonDLL = "python310.dll";

        var fileName = "test.mp3";
        if( args.Length > 0) 
            fileName = args[0];

        Console.WriteLine($"transcribing {fileName} ..");

        try
        {
            PythonEngine.Initialize(false, false);
            // TranscribeWithOpenAiPythonModule(fileName);
            TranscribeWithLocalWhisperModel(fileName); 
        }
        finally
        {
            Console.WriteLine("PythonEngine.Shutdown ..");
            PythonEngine.Shutdown();

            Console.WriteLine("Press Any Key to Exit ..");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }

    private static void TranscribeWithLocalWhisperModel(string fileName)
    {
        using (Py.GIL())
        {
            /*
            import whisper
            model = whisper.load_model("tiny.en")
            file_path = r".\audio\test.mp3"
            result = model.transcribe(file_path)
            print(result["text"])
            */            
            dynamic whisper = Py.Import("whisper");
            dynamic model = whisper.load_model("tiny.en");
            dynamic result = model.transcribe(fileName);
            Console.WriteLine(result);      
        }        
    }    

    private static void TranscribeWithOpenAiPythonModule(string fileName)
    {
        using (Py.GIL())
        {
            /*
            import os
            api_key = os.environ.get('OPENAI_API_KEY')

            import openai
            // openai.api_key_path = "./api-key.txt"
            openai.api_key = api_key
            audio_file= open(file_name, "rb")
            result = openai.Audio.transcribe("whisper-1", audio_file)
            print(result["text"])
            */

            dynamic os = Py.Import("os");
            dynamic api_key = os.environ.get("OPENAI_API_KEY");

            dynamic openai = Py.Import("openai");
            openai.api_key = api_key;

            using (dynamic scope = Py.CreateScope())
            {
                scope.Set("file_name", fileName.ToPython()); 
                string code = @"audio_file= open(file_name, 'rb')";
                scope.Exec(code);
                var audio_file = scope.Eval("audio_file");
                var result = openai.Audio.transcribe("whisper-1", audio_file);
                Console.WriteLine(result);
            }         
        }        
    }
}