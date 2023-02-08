//string rootDirectory = @"C:\code\DTAutoDev\Dashboard\Main";
string rootDirectory = @"C:\github\MethodCodeInjector\MethodCodeInjector\bin\Debug\net6.0\test";
var failedForms = new List<string>();

var designerFiles = Directory.GetFiles(rootDirectory, "*.designer.cs", SearchOption.AllDirectories);
foreach (var designerFile in designerFiles)
{
    try
    {
        AddLineToMethod(designerFile);
    }
    catch (Exception ex)
    {
        failedForms.Add(Path.GetFileNameWithoutExtension(designerFile));
    }
}

if (failedForms.Count > 0)
{
    Console.WriteLine("Failed forms: " + string.Join(", ", failedForms));
}

////////////////////////////

static void AddLineToMethod(string designerFile)
{
    string formName = Path.GetFileNameWithoutExtension(designerFile);
    string designerFileContents = File.ReadAllText(designerFile);
    string? methodName = GetMethodName(designerFileContents);

    if(methodName == null)
    {
        Console.WriteLine("No load event handler found");
        return;
    }

    // read the code file
    string codeFilePath = Path.Combine(Path.GetDirectoryName(designerFile), formName.Replace(".Designer", string.Empty) + ".cs");
    string codeFileContent = File.ReadAllText(codeFilePath);    
    
    var methodInfo = GetMethodInfo(codeFileContent, methodName);
    codeFileContent = codeFileContent.Insert(methodInfo.StartBraceIndex + 1, $"{Environment.NewLine}{methodInfo.Indentation}\tStartTest();{Environment.NewLine}");
    
    methodInfo = GetMethodInfo(codeFileContent, methodName);
    codeFileContent = codeFileContent.Insert(methodInfo.EndBraceIndex, $"{methodInfo.Indentation}\tEndTest();{Environment.NewLine}");
    File.WriteAllText(codeFilePath, codeFileContent);
    
}

// returns opening { and closing } indexes
static MethodInfo GetMethodInfo(string codeFileContent, string methodName)
{
    var methodInfo = new MethodInfo();
    
    // Search for the start of the method
    int startIndex = codeFileContent.IndexOf("private void " + methodName + "(");
    if (startIndex == -1)
    {
        methodInfo.ErrorMessage = $"Error: Method {methodName} not found";
        return methodInfo;
    }

    // Use a stack to track the opening and closing braces
    Stack<char> braceStack = new Stack<char>();
    int endIndex = -1;
    string indentation = "";
    for (int i = startIndex; i < codeFileContent.Length; i++)
    {
        if (codeFileContent[i] == '{')
        {
            braceStack.Push('{');
            if (braceStack.Count == 1)
            {
                // get whitespace count for formatting
                methodInfo.StartBraceIndex = i;
                int j = methodInfo.StartBraceIndex - 1;
                while (j >= 0 && (codeFileContent[j] == ' ' || codeFileContent[j] == '\t'))
                {
                    indentation = codeFileContent[j] + indentation;
                    j--;
                }
            }
        }
        else if (codeFileContent[i] == '}')
        {
            braceStack.Pop();
            if (braceStack.Count == 0)
            {
                methodInfo.EndBraceIndex = i;
                break;
            }
        }
    }

    methodInfo.Indentation = indentation;
    return methodInfo;
}
static string? GetMethodName(string designerFileContents)
{
    // get the load (event handler) method name
    int loadEventIndex = designerFileContents.IndexOf("this.Load += new System.EventHandler");
    if (loadEventIndex == -1)
    {
        return null;
    }

    int methodStartIndex = designerFileContents.IndexOf("(", loadEventIndex) + 1;
    int methodEndIndex = designerFileContents.IndexOf(")", methodStartIndex);
    string methodName = designerFileContents.Substring(methodStartIndex, methodEndIndex - methodStartIndex);
    methodName = methodName.Replace("this.", string.Empty);

    return methodName;
}

static int GetMethodStartIndex(string[] codeLines, string methodName)
{
    // Find the load method
    int loadMethodIndex = -1;
    for (int i = 0; i < codeLines.Length; i++)
    {
        if (codeLines[i].Contains("private void " + methodName + "("))
        {
            loadMethodIndex = i;
            break;
        }
    }

    return loadMethodIndex;
}