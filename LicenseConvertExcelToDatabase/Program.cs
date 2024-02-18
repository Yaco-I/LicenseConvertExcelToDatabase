using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;


string Administracion = "D:\\Trabajo\\Recurosos Informaticos\\LicenseConvertExcelToDatabase\\LicenseConvertExcelToDatabase\\Administracion.txt";
string Contabilidad = "D:\\Trabajo\\Recurosos Informaticos\\LicenseConvertExcelToDatabase\\LicenseConvertExcelToDatabase\\Contabilidad.txt";

List<Register> administracionList = LeerArchivoYDevolverRegistro(Administracion);


VerificarProcesosRepetidos(administracionList, false);
VerificarProcesosRepetidos(administracionList, true);

Console.Clear();


Console.WriteLine(" ---------- Administracion -------------- \n\n\n\n");
Console.WriteLine(" ---------- Procesos -------- ");
Console.WriteLine("\n");
GenerateProcessInsert(administracionList);


Console.WriteLine("-------------- Modulos----------");
GenerateModuleInsert(administracionList);
Console.WriteLine("\n");
Console.WriteLine("-------------- ModulosDet----------");
GenerateModuleDetInsert(administracionList);








void GenerateProcessInsert(List<Register> list)
{
    foreach (var register in list)
    {
        Console.WriteLine($"INSERT INTO placeholder_procesos(pproc_codigo, pproc_padre, pproc_denom) VALUES({register.NumberProcess},{FatherProcess.GetProcesoPadre(register)},'{FatherProcess.GetName()}');");
    }
}

void GenerateModuleInsert(List<Register> list)
{
    for (int i = 0; i < list.Count; i++)
    {
        if (!string.IsNullOrEmpty(list[i].Module))
        {
            list[i].ModuleCode = Register.ObtenerCodigoModulo();
            Console.WriteLine($"INSERT INTO modulos (mod_codigo, mod_denom, pproc_codigo) VALUES ({list[i].ModuleCode},'{list[i].Module}',{list[i].NumberProcess});");
        }
    }
}
void GenerateModuleDetInsert(List<Register> registers)
{
    int moduleCode = 0;
    int moduleDetItem = 1;
    foreach (var register in registers)
    {
        if (!string.IsNullOrEmpty(register.Module))
        {
            moduleCode = register.ModuleCode;
            moduleDetItem = 1;
            continue;
        }
        else if (!string.IsNullOrEmpty(register.InternalProcess))
        {
            continue;
        }


        Console.WriteLine($"INSERT INTO modulosdet(mod_codigo,modd_item,pproc_codigo) VALUES ( {moduleCode},{moduleDetItem++},{register.NumberProcess});");
    }
}


List<Register> LoadDocument(string line)
{
    string[] fields = line.Split(';');
    
    List<Register> list = new List<Register>();

    if (fields.Length != 8)
    {
        throw new Exception("The file is not in the correct format.");
    }
    if (string.IsNullOrEmpty(fields[7]))
    {
        bool floatt = true;
        foreach (var field in fields)
        {
            if (!string.IsNullOrEmpty(field))
            {
                floatt = false;
                break;
            }
        }
        if (!floatt)
        {

            fields[7] = Register.GetProcessNumber().ToString();

        }
    }

    for (int i = fields.Length - 2; i >= 0; i--)
    {
        if (!string.IsNullOrEmpty(fields[i]))
        {
            Register register = new Register
            {
                NumberProcess = list.Count >= 1 ? (Register.GetProcessNumber()).ToString() : fields[7]
            };

            switch (i)
            {
                case 6:
                    register.InternalProcess = fields[6];
                    break;
                case 5:
                    register.ModuleDet5 = fields[5];
                    break;
                case 4:
                    register.ModuleDet4 = fields[4];
                    break;
                case 3:
                    register.ModuleDet3 = fields[3];
                    break;
                case 2:
                    register.ModuleDet2 = fields[2];
                    break;
                case 1:
                    register.ModuleDet1 = fields[1];
                    break;
                case 0:
                    register.Module = fields[0];
                    break;
            }
            list.Insert(0, register);


        }

    }
    return list;

}
void VerificarProcesosRepetidos(List<Register> list, bool mostrar)
{
    List<string> procesos = new List<string>();
    List<string> repetidos = new List<string>();
    List<Register> lista = new List<Register>();
    foreach (var registro in list)
    {
        if (!procesos.Contains(registro.NumberProcess))
        {
            procesos.Add(registro.NumberProcess);
            lista.Add(registro);
        }
        else
        {
            repetidos.Add(registro.NumberProcess);
            if (mostrar)
            {
                Console.WriteLine($"{registro}");
                Console.WriteLine(lista.FirstOrDefault(x => x.NumberProcess == registro.NumberProcess));
            }
        }
    }

    if (mostrar)
    {
        for (int i = 0; i < procesos.Count; i++)
        {
            for (int j = i + 1; j < procesos.Count; j++)
            {
                if (procesos[i] == procesos[j])
                {
                    Console.WriteLine($"El proceso {procesos[i]} esta repetido mas de una vez.");
                }
            }
        }
    }



    foreach (var proceso in procesos)
    {
        List<int> indexProcRepetido = new List<int>();
        for (int i = 0; list.Count > i; i++)
        {
            if (list[i].NumberProcess == proceso)
            {
                indexProcRepetido.Add(i);
            }
        }

        if (indexProcRepetido.Count > 1)
        {
            var result = CambiarId(list[indexProcRepetido[0]], list[indexProcRepetido[1]]);
            list[indexProcRepetido[0]] = result[0];
            list[indexProcRepetido[1]] = result[1];
        }
    }

    

}

List<Register> CambiarId(Register reg1, Register reg2)
{
    var result = reg1.ComparetTo(reg2);

    if (result == 1)
    {
        reg1.NumberProcess = Register.GetProcessNumber().ToString();
    }
    else if (result == -1)
    {
        reg2.NumberProcess = Register.GetProcessNumber().ToString();
    }

    return new List<Register>() { reg1, reg2 };
}
List<Register> LeerArchivoYDevolverRegistro(string filePath)
{

    List<Register> list = new List<Register>();
    try
    {
        //string filePath = "D:\\Trabajo\\Recurosos Informaticos\\LicenseConvertExcelToDatabase\\LicenseConvertExcelToDatabase\\Administracion.txt";

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    list.AddRange(LoadDocument(line));
                }
            }
        }
        else
        {
            Console.WriteLine("El archivo no existe en la ruta especificada.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }


    return list;


}




public class Register
{
    private static int ProcessNumber = 0;
    public static int GetProcessNumber() => ++ProcessNumber;
    
    private static int NewCodeModule = 0;
    public static int ObtenerCodigoModulo() => ++NewCodeModule;




    public string Module { get; set; }
    public string ModuleDet1 { get; set; }
    public string ModuleDet2 { get; set; }
    public string ModuleDet3 { get; set; }
    public string ModuleDet4 { get; set; }
    public string ModuleDet5 { get; set; }
    public string InternalProcess { get; set; }
    public string NumberProcess { get; set; }

    public int ModuleCode { get; set; }




    /// <summary>
    /// Si devuelve 0 es que son iguales,
    /// si devuelve -1 es que el objeto que llama es menor que el objeto que se le pasa por parametro,
    /// si devuelve 1 es que el objeto que llama es mayor que el objeto que se le pasa por parametro.
    /// </summary>
    /// <param name="registro"></param>
    /// <returns></returns>
    public int ComparetTo(Register registro)
    {
        if (Module != registro.Module)
        {
            return compareAttribute(Module, registro.Module);
        }
        if (ModuleDet1 != registro.ModuleDet1)
        {
            return compareAttribute(ModuleDet1, registro.ModuleDet1);
        }
        if (ModuleDet2 != registro.ModuleDet2)
        {
            return compareAttribute(ModuleDet2, registro.ModuleDet2);
        }
        if (ModuleDet3 != registro.ModuleDet3)
        {
            return compareAttribute(ModuleDet3, registro.ModuleDet3);
        }
        if (ModuleDet4 != registro.ModuleDet4)
        {
            return compareAttribute(ModuleDet4, registro.ModuleDet4);
        }
        if (ModuleDet5 != registro.ModuleDet5)
        {
            return compareAttribute(ModuleDet5, registro.ModuleDet5);
        }
        if (InternalProcess != registro.InternalProcess)
        {
            return 0;
        }

        return 0;
    }

    private int compareAttribute(string proc1, string proc2)
    {
        if (!string.IsNullOrEmpty(proc1) && !string.IsNullOrEmpty(proc2))
            return 0;
        else if (!string.IsNullOrEmpty(proc1))
            return 1;
        else
            return -1;
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(Module))
            sb.Append($"Module: {Module}");
        if (!string.IsNullOrEmpty(ModuleDet1))
            sb.Append($"ModuleDet1: {ModuleDet1}");

        if (!string.IsNullOrEmpty(ModuleDet2))
            sb.Append($"ModuleDet2: {ModuleDet2}");
        if (!string.IsNullOrEmpty(ModuleDet3))
            sb.Append($"ModuleDet3: {ModuleDet3}");
        if (!string.IsNullOrEmpty(ModuleDet4))
            sb.Append($"ModuleDet4: {ModuleDet4}");
        if (!string.IsNullOrEmpty(ModuleDet5))
            sb.Append($"ModuleDet5: {ModuleDet5}");

        return sb.ToString();
    }
}


public class FatherProcess
{

    private static string ProcesoNumeroModulo { get; set; }
    private static string ProcessModuleName { get; set; }
    private static string ProcesoNumeroModuloDet1 { get; set; }
    private static string ProcessModuleDet1Name { get; set; }
    private static string ProcesoNumeroModuloDet2 { get; set; }
    private static string ProcessModuleDet2Name { get; set; }
    private static string ProcesoNumeroModuloDet3 { get; set; }
    private static string ProcessModuleDet3Name { get; set; }
    private static string ProcesoNumeroModuloDet4 { get; set; }
    private static string ProcessModuleDet4Name { get; set; }
    private static string ProcesoNumeroModuloDet5 { get; set; }
    private static string ProcessModuleDet5Name { get; set; }
    private static string InternalProcessName { get; set; }


    static void SetProcesoModulo(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModulo = numeroProceso;
        ProcessModuleName = nameProcess;
        SetProcesoModuloDet1(string.Empty, string.Empty);
        
    }
    static void SetProcesoModuloDet1(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModuloDet1 = numeroProceso;
        ProcessModuleDet1Name = nameProcess;
        SetProcesoModuloDet2(string.Empty, string.Empty);
        
    }
   static void SetProcesoModuloDet2(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModuloDet2 = numeroProceso;
        ProcessModuleDet2Name = nameProcess;
        SetProcesoModuloDet3(string.Empty, string.Empty);
    }
    static void SetProcesoModuloDet3(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModuloDet3 = numeroProceso;
        ProcessModuleDet3Name = nameProcess;
        SetProcesoModuloDet4(string.Empty, string.Empty); 
    }
    static void SetProcesoModuloDet4(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModuloDet4 = numeroProceso;
            ProcessModuleDet4Name = nameProcess;
        SetProcesoModuloDet5(string.Empty, string.Empty);
      
    }
    static void SetProcesoModuloDet5(string numeroProceso, string nameProcess)
    {
        ProcesoNumeroModuloDet5 = numeroProceso;
        ProcessModuleDet5Name = nameProcess;
        InternalProcessName = string.Empty;
        
    }


    static string GetNumberInternalProcess() => string.IsNullOrEmpty(ProcesoNumeroModuloDet5) ? GetNumberModuleDet5() : ProcesoNumeroModuloDet5;
    
    static string GetNumberModuleDet5() => string.IsNullOrEmpty(ProcesoNumeroModuloDet4) ? GetNumberModuleDet4() : ProcesoNumeroModuloDet4;
    
    static string GetNumberModuleDet4() => string.IsNullOrEmpty(ProcesoNumeroModuloDet3) ? GetNumberModuleDet3() : ProcesoNumeroModuloDet3;
    

    static string GetNumberModuleDet3() => string.IsNullOrEmpty(ProcesoNumeroModuloDet2) ? GetNumberModuleDet2() : ProcesoNumeroModuloDet2;

    static string GetNumberModuleDet2() => string.IsNullOrEmpty(ProcesoNumeroModuloDet1) ? GetNumberModuleDet1() : ProcesoNumeroModuloDet1;

    static string GetNumberModuleDet1() => ProcesoNumeroModulo;
    

    private static string GetProcessNumber()
    {
        string number = string.Empty;

        if(!string.IsNullOrEmpty(InternalProcessName))
            number =  GetNumberInternalProcess();
        else if (!string.IsNullOrEmpty(ProcesoNumeroModuloDet5))
            number = GetNumberModuleDet5();
        else if (!string.IsNullOrEmpty(ProcesoNumeroModuloDet4))
            number = GetNumberModuleDet4();
        else if (!string.IsNullOrEmpty(ProcesoNumeroModuloDet3))
            number = GetNumberModuleDet3();
        else if (!string.IsNullOrEmpty(ProcesoNumeroModuloDet2))
            number = GetNumberModuleDet2();
        else if (!string.IsNullOrEmpty(ProcesoNumeroModuloDet1))
            number = GetNumberModuleDet1();

        return number;
    }

    public static string GetName()
    {
        StringBuilder sb = new StringBuilder();

        if(!string.IsNullOrEmpty(ProcessModuleName))
            sb.Append(ProcessModuleName);

        if (!string.IsNullOrEmpty(ProcessModuleDet1Name))
            sb.Append($"_{ProcessModuleDet1Name}");

        if (!string.IsNullOrEmpty(ProcessModuleDet2Name))
            sb.Append($"_{ProcessModuleDet2Name}");
        if (!string.IsNullOrEmpty(ProcessModuleDet3Name))
            sb.Append($"_{ProcessModuleDet3Name}");
        if (!string.IsNullOrEmpty(ProcessModuleDet4Name))
            sb.Append($"_{ProcessModuleDet4Name}");
        if (!string.IsNullOrEmpty(ProcessModuleDet5Name))
            sb.Append($"_{ProcessModuleDet5Name}");
        if (!string.IsNullOrEmpty(InternalProcessName))
            sb.Append($"_{InternalProcessName}");
        return sb.ToString();
    }

    public static string GetProcesoPadre(Register registro)
    {
        string value = string.Empty;
        if(!string.IsNullOrEmpty(registro.Module))
            SetProcesoModulo(registro.NumberProcess, registro.Module);
        else if(!string.IsNullOrEmpty(registro.ModuleDet1))
            SetProcesoModuloDet1(registro.NumberProcess, registro.ModuleDet1);
        else if (!string.IsNullOrEmpty(registro.ModuleDet2))
            SetProcesoModuloDet2(registro.NumberProcess, registro.ModuleDet2);
        else if (!string.IsNullOrEmpty(registro.ModuleDet3))
            SetProcesoModuloDet3(registro.NumberProcess, registro.ModuleDet3);
        else if (!string.IsNullOrEmpty(registro.ModuleDet4))
            SetProcesoModuloDet4(registro.NumberProcess, registro.ModuleDet4);
        else if (!string.IsNullOrEmpty(registro.ModuleDet5))
            SetProcesoModuloDet5(registro.NumberProcess, registro.ModuleDet5);
        else if(!string.IsNullOrEmpty(registro.InternalProcess))
            InternalProcessName = registro.InternalProcess;

        value = GetProcessNumber();

        if(string.IsNullOrEmpty(value))
        {
            Console.WriteLine(registro);
        }

        return string.IsNullOrEmpty(value) ? "NULL" : value;
    }


}
