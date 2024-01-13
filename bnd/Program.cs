using System.CommandLine;
using System.Formats.Asn1;


static string[] AllEndings(string language, string[] allLanguages, string[] endings)
{
    if (language.Equals("all"))
        return endings;
    string[] languages = language.Split(' ');
    for (int i = 0; i < languages.Length; i++)
    {
        for (int j = 0; j < allLanguages.Length; j++)
        {
            if (languages[i].Equals(allLanguages[j]))
            {
                languages[i] = endings[j];
                break;
            }
        }
    }
    return languages;
}
var bundleCommand = new Command("bundle", "bundle code to file");
var bundleOptionOutput = new Option<FileInfo>(new[] { "--output", "-o" }, "file path and name");
bundleCommand.AddOption(bundleOptionOutput);
var bundleOptionLanguage = new Option<string>(new[] { "--language", "-l" }, "list of languages");
bundleCommand.AddOption(bundleOptionLanguage);
var bundleOptionNote = new Option<bool>(new[] { "--note", "-n" }, () => false, "note the source code");
bundleCommand.AddOption(bundleOptionNote);
var bundleOptionSort = new Option<string>(new[] { "--sort", "-s" }, () => "letter", "order of copying the code files");
bundleCommand.AddOption(bundleOptionSort);
var bundleOptionRemoveEmptyLines = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, () => false, "delete empty lines");
bundleCommand.AddOption(bundleOptionRemoveEmptyLines);
var bundleOptionAuthor = new Option<string>(new[] { "--author", "-a" }, "registering the name of the creator of the file");
bundleCommand.AddOption(bundleOptionAuthor);
var createRspCommand = new Command("create-rsp", "Create a response file with a ready command");
string[] arrLanguage = { "c#", "c++", "c", "java", "python", "javaScript", "html", "css" };
string[] arrEndings = { ".cs", ".cpp", ".c", ".java", ".py", ".js", ".html", ".css" };

bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
{
    string[] endings = AllEndings(language, arrLanguage, arrEndings);
    string path = Directory.GetCurrentDirectory();
    List<string> Folders = Directory.GetFiles(path, "", SearchOption.AllDirectories).Where(file => !file.Contains("bin") && !file.Contains("Debug")).ToList();
    string[] files = Folders.Where(f => endings.Contains(Path.GetExtension(f))).ToArray();

    try
    {
        if (files.Any())
        {
            using (var bundleFile = new StreamWriter(output.FullName, false))
            {
                if (!string.IsNullOrEmpty(author))
                    bundleFile.WriteLine("#Author: " + author);
                if (note)            // רשומת הערה עם מקור הקוד
                    bundleFile.WriteLine($"# Source code from: {path}\n");
                foreach (string file in files)
                {


                    if (note)
                        bundleFile.WriteLine($"#this Source code from: {file}\n");

                    var sourceCode = File.ReadAllLines(file);

                    if (remove)
                        sourceCode = sourceCode.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                    if (sort == "t")
                        Array.Sort(files, (a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));

                    else
                        Array.Sort(files);

                    foreach (var line in sourceCode)
                        bundleFile.WriteLine(line);

                    bundleFile.WriteLine();

                }
                Console.WriteLine($"Bundle created successfully at: {Directory.GetCurrentDirectory()}");
            }
        }

    }
    catch (DirectoryNotFoundException d)
    {
        Console.WriteLine("Error: file path invalid");
    }

}, bundleOptionOutput, bundleOptionLanguage, bundleOptionNote, bundleOptionSort, bundleOptionRemoveEmptyLines, bundleOptionAuthor);

createRspCommand.SetHandler(() =>
{
    var response = new FileInfo("response.rsp");

    using (StreamWriter rspFile = new StreamWriter(response.FullName))
    {
        Console.WriteLine("enter path and name");
        var path = Console.ReadLine();
        rspFile.WriteLine("--output " + path);

        Console.WriteLine("Choose a language, if you want in all languages ​​you have the option to write all");
        var language = Console.ReadLine();
        rspFile.WriteLine("--language "+language);

        Console.WriteLine("Whether to list the source code as a comment in the bundle file. (yes/no)");
        var note = Console.ReadLine();
        rspFile.WriteLine(note == "yes" ? "--note" : "");

        Console.WriteLine("The order of copying the code files, according to the alphabet of the file name or according to the type of code. press t to type");
        var sort = Console.ReadLine();
        rspFile.WriteLine(sort=="t"?"--sort"+sort:"");


        Console.WriteLine("Do delete empty lines?  (yes/no)");
        var remove = Console.ReadLine();
        rspFile.WriteLine(remove == "yes" ? "--remove-empty-lines " : "");

        Console.WriteLine("Registering the name of the creator of the file.");
        var author = Console.ReadLine();
        rspFile.WriteLine(author!=""? "--author "+ author:"");

    }

});

var rootCommand = new RootCommand("root for bundle files");
rootCommand.AddCommand(createRspCommand);
rootCommand.AddCommand(bundleCommand);
rootCommand.InvokeAsync(args);