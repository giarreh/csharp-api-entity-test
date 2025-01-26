using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.DTOs;
using workshop.wwwapi.Models;
using workshop.wwwapi.Repository;
using workshop.wwwapi.ViewModel;

namespace workshop.wwwapi.Endpoints
{
    public static class SurgeryEndpoint
    {
        //TODO:  add additional endpoints in here according to the requirements in the README.md 
        public static void ConfigurePatientEndpoint(this WebApplication app)
        {
            var surgeryGroup = app.MapGroup("surgery");

            surgeryGroup.MapGet("/patients", GetPatients);
            surgeryGroup.MapGet("/patients/{id}", GetPatientById);
            surgeryGroup.MapPost("/patients", AddPatient);
            surgeryGroup.MapGet("/doctors", GetDoctors);
            surgeryGroup.MapGet("/doctors/{id}", GetDoctorById);
            surgeryGroup.MapPost("/appointments/{doctorid}/{patientId}", CreateAppointment);
            surgeryGroup.MapGet("/appointments", GetAppointmentsAsync);
            surgeryGroup.MapGet("/appointmentsbydoctor/{id}", GetAppointmentsByDoctor);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPatients(IRepository repository)
        {
            List<PatientDTO> patientsDTO = new List<PatientDTO>();

            var patients = await repository.GetPatients();
            foreach (var patient in patients)
            {
                // Create a new list for each patient to store their appointments
                List<AppointmentWithDoctor> appointments = new List<AppointmentWithDoctor>();

                var patientAppointments = await repository.GetAppointmentsByPatient(patient.Id);
                foreach (var appointment in patientAppointments)
                {
                    Doctor doc = await repository.GetDoctorById(appointment.DoctorId);

                    AppointmentWithDoctor appointmentDTO = new AppointmentWithDoctor
                    {
                        DoctorId = appointment.DoctorId,
                        Booking = appointment.Booking,
                        DoctorName = doc.FullName
                    };

                    appointments.Add(appointmentDTO);
                }

                PatientDTO dto = new PatientDTO
                {
                    Id = patient.Id,
                    Name = patient.FullName,
                    appointments = appointments
                };

                patientsDTO.Add(dto);
            }

            return Results.Ok(patientsDTO);
        }



        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetDoctors(IRepository repository)
        {
            List<DoctorDTO> doctorsDTO = new List<DoctorDTO>();
            var doctors = await repository.GetDoctors();
            foreach (var doctor in doctors)
            {
                List<AppointmentWithPatient> appointments = new List<AppointmentWithPatient>();

                var doctorappointments = await repository.GetAppointmentsByDoctor(doctor.Id);
                foreach (var appointment in doctorappointments)
                {
                    Patient patient = await repository.GetPatientById(appointment.PatientId);

                    AppointmentWithPatient appointmentDTO = new AppointmentWithPatient
                    {
                        PatientId = appointment.PatientId,
                        PatientName = patient.FullName,
                        Booking = appointment.Booking
                    };

                    appointments.Add(appointmentDTO);
                }

                DoctorDTO dto = new DoctorDTO
                {
                    Id = doctor.Id,
                    Name = doctor.FullName,
                    appointments = appointments
                };

                doctorsDTO.Add(dto);
            }

            return TypedResults.Ok(doctorsDTO);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetDoctorById(IRepository repository, int id)
        {
            return TypedResults.Ok(await repository.GetDoctorById(id));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetAppointmentsAsync(IRepository repository)
        {
            return TypedResults.Ok(await repository.GetAppointmentsAsync());
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetAppointmentsByDoctor(IRepository repository, int id)
        {
            Doctor doctor = await repository.GetDoctorById(id);

            DoctorDTO doctorDTO = new DoctorDTO { Id = doctor.Id, Name = doctor.FullName };

            return TypedResults.Ok(await repository.GetAppointmentsByDoctor(id));
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        public static async Task<IResult> CreateAppointment(IRepository repository, int doctorId, int patientId)
        {
            Doctor doctor = await repository.GetDoctorById(doctorId);
            Patient patient = await repository.GetPatientById(patientId);

            AppointmentDTOwithName appointment = new AppointmentDTOwithName();

            Appointment resp = await repository.CreateAppointment(doctorId, patientId);

            appointment.PatientName = patient.FullName;
            appointment.PatientId = patient.Id;
            appointment.DoctorId = doctor.Id;
            appointment.DoctorName = doctor.FullName;
            appointment.Booking = resp.Booking;
            return TypedResults.Created($"/surgery/appointments/{appointment.DoctorId}/{appointment.PatientId}", appointment);
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetPatientById(IRepository repository, int id)
        {
            Patient patient = await repository.GetPatientById(id);

            if(patient == null)
            {
                return TypedResults.NotFound($"Patient with ID: {id} was not found");
            }

            PatientDTO patientDTO = new PatientDTO() { Id = patient.Id, Name = patient.FullName };
            return TypedResults.Ok(patient);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        public static async Task<IResult> AddPatient(IRepository repository, PatientPost patientPost)
        {
            Patient patient = await repository.AddPatient(new Patient() { FullName = patientPost.Name});
            return TypedResults.Created($"/patients/{patient.Id}", patient);
        }
    }
}
