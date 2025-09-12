using System.Linq.Expressions;
using System.Net;

List<Patient?> patients = new List<Patient?>();
List<Physician?> physicians = new List<Physician?>();
List<Appointment?> appointments = new List<Appointment?>();

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
		createAppointment(patients, physicians, appointments);
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
		Console.WriteLine("R. Read Patients");
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

void physicianFunction(List<Physician?> physicians)
{
	bool physician_loop = true;
	do
	{
		Console.WriteLine("\nC. Create a new Physician");
		Console.WriteLine("A. Add a Specialization");
		Console.WriteLine("R. Read Physicians");
		Console.WriteLine("B. Back");
		input = Console.ReadLine();
		switch (input)
		{
		case "C":
		case "c":
			var newPhysician = createPhysician(physicians);
			physicians.Add(newPhysician);
			Console.WriteLine("Physician created.");
			break;
		case "A":
		case "a":
			if (physicians.Any())
				addSpecialization(physicians);
			else
				Console.WriteLine("No physicians have been created.");
			break;
		case "R":
		case "r":
			foreach (var reader in physicians)
			{
				Console.WriteLine($"({reader?.Id}) {reader?.name}, {reader?.graduation.ToString("dd/MM/yyyy")}");
				foreach (var specialization in reader?.specializations)
				{
					Console.WriteLine($"    - {specialization}");
				}
			}
			break;
		case "B":
		case "b":
			physician_loop = false;
			break;
		}
	} while (physician_loop == true);
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
	return physician;
}

void addSpecialization(List<Physician?> physicians)
{
	Console.WriteLine("Please Choose the physicians ID:");
	foreach (var specialization in physicians)
	{
		Console.WriteLine($"({specialization?.Id}) {specialization?.name}");
	}

	int user_id = -1;
	user_id = int.Parse(Console.ReadLine());

	if (physicians[user_id - 1] != null)
	{
		var chosen_physician = physicians[user_id - 1];
		Console.WriteLine("Type in the physicians specialization:");
		string? specialization_input = Console.ReadLine();
		chosen_physician.specializations.Add(specialization_input);
		Console.WriteLine("Specialization added.");
	}
	else
	{
		Console.WriteLine("Invalid ID.");
	}
}

void createAppointment(List <Patient?> patients, List<Physician?> physicians, List<Appointment?> appointments)
{
	if (!(patients.Any() && physicians.Any()))
	{
		Console.WriteLine("Please add at least one patient and one physician to create an appointment.");
		return;  
	}

	var newAppointment = new Appointment();
	var chosen_patient = new Patient();
	string print_line = ""; 

	// Prints out all of the patients + their medical notes
	Console.WriteLine("Which patient would you like to schedule an appointment with?");
	foreach (var patient in patients)
	{
		print_line = ""; 
		print_line += $"Patient: ({patient?.Id}) {patient?.name}";
		print_line += (patient?.gender?.ToLower() == "m") ? " 🧔\n" : " 👩\n";

		if (patient.medical_notes.Any())
		{
			print_line += "-----\n";
			print_line += "Medical Notes:\n";
			foreach (var medical_note in patient?.medical_notes)
			{
				print_line += $"- {medical_note}\n";
			}
		}

		if (patient.unavailable_hours.Any())
		{
			print_line += "-----\n";
			print_line += "Scheduled Appointments:\n";
			foreach (var unavailable_hour in patient?.unavailable_hours)
			{
				print_line += $"- {unavailable_hour}\n";
			}
		}
		
		print_line += "==============================\n";
		Console.WriteLine(print_line);
	}

	// Logic to choose the user id
	int user_id = 0;
	user_id = int.Parse(Console.ReadLine());
	bool patient_found = false;
	do
	{
		try
		{
			chosen_patient = patients[user_id - 1];
			patient_found = true;
		}
		catch
		{
			Console.WriteLine("Invalid ID. Please choose a valid ID. ");
			user_id = int.Parse(Console.ReadLine());
		}
	} while (patient_found == false);


	// Logic to chose the date
	Console.WriteLine("What is the appointment date? (MM/DD/YYYY)");
	bool isValidDate = false;
	DateTime appointmentStartDate = DateTime.MinValue;
	DateTime finalAppointmentTime = DateTime.MinValue;

	do
	{
		try
		{
			appointmentStartDate = DateTime.Parse(Console.ReadLine());
			isValidDate = true;
		}
		catch
		{
			Console.WriteLine("Invalid date. Please choose a valid date. (MM/DD/YYYY)");
		}
	} while (isValidDate == false);

	DateTime loop_start_time = appointmentStartDate.Date.AddHours(8);
	DateTime loop_end_time = appointmentStartDate.Date.AddHours(17);

	print_line = ""; 
	// For every hour, print a physician. IF unavailable_hour = current_hour then don't print that physician for that hour
	for (DateTime current_hour = loop_start_time; current_hour <= loop_end_time; current_hour = current_hour.AddHours(1))
	{
		bool patient_is_available = true;
			if (chosen_patient.unavailable_hours.Any())
			{
				foreach (var unavailable_hour in chosen_patient?.unavailable_hours)
				{
					if (unavailable_hour.Date == current_hour.Date && unavailable_hour.Hour == current_hour.Hour)
					{
						patient_is_available = false; 
					}
				}
			}
				print_line = current_hour.ToString("hh:mm tt");

		if (patient_is_available == true)
		{
			foreach (var physician in physicians)
			{
				bool physician_is_available = true;
				if (physician.unavailable_hours.Any())
				{
					foreach (var unavailable_hour in physician?.unavailable_hours)
					{
						if (unavailable_hour.Hour == current_hour.Hour && unavailable_hour.Date == current_hour.Date)
						{
							physician_is_available = false;
						}
					}
				}
				if (physician_is_available == true)
				{
					print_line += $" | ({physician?.Id}) {physician?.name}";
				}
			}
		}

		else
		{
			print_line += " | Unavailable"; 
		}
		Console.WriteLine(print_line);
	}

	bool chose_valid_physician = false;
	int chosenHour = 0;
	do
	{
		// This section is for the user to select the time
		Console.WriteLine("Please choose the time for the appointment. (8-17)");
		bool isValidHour = true;
		var patient_unavailable_hours = new List<DateTime>(patients[user_id - 1].unavailable_hours);
		do
		{
			isValidHour = true;
			try
			{
				chosenHour = int.Parse(Console.ReadLine());

				if (!(chosenHour >= 8 && chosenHour <= 17))
				{
					isValidHour = false;
					Console.WriteLine("Invalid hour. Please choose a value between 8 and 17.");
				}

				if (patient_unavailable_hours.Any())
				{
					foreach (var patient_unavailable_hour in patient_unavailable_hours)
					{
						if (chosenHour == patient_unavailable_hour.Hour && appointmentStartDate.Date == patient_unavailable_hour.Date)
						{
							isValidHour = false;
							Console.WriteLine("Invalid hour. Please choose a time when the patient is available.");
						}
					}
				}
			}
			catch
			{
				Console.WriteLine("Invalid input. Please enter a number.");
			}
		} while (isValidHour == false);

		// Combine the user's date choice and their chosen hour
		finalAppointmentTime = appointmentStartDate.Date.AddHours(chosenHour);
		Console.WriteLine(finalAppointmentTime);

		// Prints the physicians
		print_line = "";
		foreach (var physician in physicians)
		{
			bool isAvailable = true;
			foreach (var unavailable_hour in physician?.unavailable_hours)
			{
				if (unavailable_hour.Date == finalAppointmentTime.Date && unavailable_hour.Hour == finalAppointmentTime.Hour)
				{
					isAvailable = false;
				}
			}
			if (isAvailable == true)
			{
				print_line += $"({physician?.Id}) {physician?.name}  | ";
			}
		}

		if (print_line == "")
		{
			Console.WriteLine($"Error the time slot that you've chosen ({finalAppointmentTime}) is already full with physicians.");
		}

		else
		{
			Console.WriteLine("Which physician would you like to schedule the appointment with?");
			chose_valid_physician = true;
			Console.WriteLine(print_line);
		}

	} while (chose_valid_physician == false);


	// Choses the physician by the id
	int physician_id = -1;
	physician_id = int.Parse(Console.ReadLine());
	bool chose_physician = false;

	do
	{
		foreach (var physician in physicians)
		{
			if (physician?.Id == physician_id)
			{
				chose_physician = true;
				chosen_patient.unavailable_hours.Add(finalAppointmentTime);
				physician.unavailable_hours.Add(finalAppointmentTime);
				newAppointment.patients = chosen_patient;
				newAppointment.physicians = physician;
				appointments.Add(newAppointment);
				Console.WriteLine("Appointment created.");
				return;
			}
		}
		Console.WriteLine("Error did not choose correct physician ID. Please chose correct ID.");
		physician_id = int.Parse(Console.ReadLine());
	} while (chose_physician == false);
}