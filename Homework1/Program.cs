using System.Linq.Expressions;
using System.Net;

List<Patient?> patients = new List<Patient?>();
List<Physician?> physicians = new List<Physician?>();

string? input;

bool continue_loop = true;
do
{
  Console.WriteLine("\nP. Patient Status");
  Console.WriteLine("H. Physician Status");
  Console.WriteLine("A. Add a new Appointment");
  Console.WriteLine("Q. Quit");
  input = Console.ReadLine();
  switch (input)
  {
    case "P":
    case "p":
      patientFunction(patients);
      break;
    case "H":
    case "h":
      physicianFunction(physicians);
      break;
    case "A":
    case "a":
      break;
    case "Q":
    case "q":
      continue_loop = false;
      break;
  }

} while (continue_loop == true);

void patientFunction(List<Patient?> patients)
{
  bool patient_loop = true;
  do
  {
    Console.WriteLine("\nC. Create a new Patient");
    Console.WriteLine("A. Add medical notes to a Patient");
    Console.WriteLine("R. Read a Patients");
    Console.WriteLine("B. Back");
    input = Console.ReadLine();
    switch (input)
    {
      case "C":
      case "c":
        Patient newPatient = createPatient(patients);
        patients.Add(newPatient);
        break;
      case "A":
      case "a":
        if (patients.Any())
          addMedicalNote(patients);
        else
          Console.WriteLine("No patients have been created.");
        break;
      case "R":
      case "r":
        foreach (var reader in patients)
        {
          Console.WriteLine($"({reader?.Id}) {reader?.name}, {reader?.address}, {reader?.birthdate.ToString("dd/MM/yyyy")}, {reader?.race}, {reader?.gender}");
          foreach (var medical_note in reader?.medical_notes)
          {
            Console.WriteLine($"    - {medical_note}");
          }
        }
        break;
      case "B":
      case "b":
        patient_loop = false;
        break;
    }
  } while (patient_loop == true);
}

void physicianFunction(List<Physician?> physicians)
{
  bool physician_loop = true;
		do
		{
			Console.WriteLine("\nC. Create a new Physician");
			Console.WriteLine("R. Retrieve a Physician");
			Console.WriteLine("B. Back");
			input = Console.ReadLine();
			switch (input)
			{
			case "C":
			case "c":
				var newPhysician = createPhysician(physicians);
				physicians.Add(newPhysician);
				break;
			case "R":
			case "r":
				foreach (var reader in physicians)
				{
					Console.WriteLine($"({reader?.Id}) {reader?.name}, {reader?.graduation.ToString("dd/MM/yyyy")}, {reader?.specialization}");
				}
				break;
			case "B":
			case "b":
				physician_loop = false;
				break;
			}
		} while (physician_loop == true);
}

Patient createPatient(List <Patient?> patients)
{
	var patient = new Patient();

	var maxId = -1;

	if (patients.Any())
	{
		maxId = patients.Select(reader => reader?.Id ?? -1).Max();
	}
	else
	{
		maxId = 0;
	}
	patient.Id = ++maxId;

	Console.WriteLine("What is the patients name?");
	string? name = Console.ReadLine();
	patient.name = name;


	Console.WriteLine("What is the patients address?");
	string? address = Console.ReadLine();
	patient.address = address;

	Console.WriteLine("What is the patients birthdate? (MM/DD/YYYY)");
	bool isValidDate = false;
	do
	{
		try
		{
			DateTime birthdate = DateTime.Parse(Console.ReadLine());
			patient.birthdate = birthdate;
			isValidDate = true;
		}
		catch
		{
			Console.WriteLine("Invalid format. Please use MM/DD/YYYY.");
		}
	} while (isValidDate == false);


	Console.WriteLine("What is the patients race?");
	string? race = Console.ReadLine();
	patient.race = race;

	Console.WriteLine("What is the patients gender? (M/F)");
	bool isValidGender = false;
	do
	{
		string? gender = Console.ReadLine();
		if (gender == "M" || gender == "m" || gender == "F" || gender == "f")
		{
			patient.gender = gender;
			isValidGender = true;
		}
		else
		{
			Console.WriteLine("Invalid Gender.");
		}
	} while (isValidGender == false);

	return patient;
}

Physician createPhysician(List<Physician?> physicians)
{

	var physician = new Physician();

	var maxId = -1;

	if (physicians.Any())
	{
		maxId = physicians.Select(reader => reader?.Id ?? -1).Max();
	}
	else
	{
		maxId = 0;
	}
	physician.Id = ++maxId;

	Console.WriteLine("What is the physician's name?");
	string? name = Console.ReadLine();
	physician.name = name;

	Console.WriteLine("What is the physicians graduation date? (MM/DD/YYYY)");
	bool isValidDate = false;
	do
	{
		try
		{
			DateTime graduation = DateTime.Parse(Console.ReadLine());
			physician.graduation = graduation;
			isValidDate = true;
		}
		catch
		{
			Console.WriteLine("Invalid format. Please use MM/DD/YYYY.");
		}
	} while (isValidDate == false);

	Console.WriteLine("What is the physicians specialization?");
	string? specialization = Console.ReadLine();
	physician.specialization = specialization;

	return physician;
}

void addMedicalNote(List<Patient?> patients)
{
  Console.WriteLine("Please Choose the patients ID:");

  foreach (var reader in patients)
  {
    Console.WriteLine($"({reader?.Id}) {reader?.name}");
  }
  int user_id = -1;
  user_id = int.Parse(Console.ReadLine());

  if (patients[user_id - 1] != null)
  {
    var chosen_patient = patients[user_id - 1];
    Console.WriteLine("Type in the patients medical note:");
    string? medical_input = Console.ReadLine();
    chosen_patient.medical_notes.Add(medical_input);
    Console.WriteLine("Medical note added.");
  }
  else
  {
    Console.WriteLine("Invalid ID.");
  }
}