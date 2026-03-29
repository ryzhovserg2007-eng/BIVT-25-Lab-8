using System.Xml.Linq;

namespace Lab8.Green
{
    public class Task1
    {
        public abstract class Participant
        {
            private string _surname;
            private string _group;
            private string _trainer;
            private double _result;
            protected double Norm;
            private static int _passed;
            
            public string Surname => _surname;
            public string Group => _group;
            public string Trainer => _trainer;
            public double Result => _result;
            public static int PassedTheStandard => _passed;

            public bool HasPassed
            {
                get
                {
                    if (_result == 0)
                        return false;

                    return _result <= Norm;
                }
            }

            static Participant()
            {
                _passed = 0;
            }
            public Participant(string surname, string group, string trainer)
            {
                _surname = surname;
                _group = group;
                _trainer = trainer;
            }

            public void Run(double result)
            {   
                if(_result == 0)
                {
                    _result = result;

                    if (result <= Norm)
                        _passed++;
                }
            }

            public static Participant[] 
                GetTrainerParticipants(Participant[] participants, Type participantType, string trainer)
            {
                Participant[] newArray = [];
                
                foreach (var participant in participants)
                {
                    if (participant.Trainer == trainer && participant.GetType() == participantType)
                    {
                        Array.Resize(ref newArray, newArray.Length + 1);
                        newArray[newArray.Length - 1] = participant;
                    }
                }
                
                return newArray;
            }

            public void Print()
            {
                Console.WriteLine($"Фамилия: {Surname}");
                Console.WriteLine($"Группа: {Group}");
                Console.WriteLine($"Тренер: {Trainer}");
                Console.WriteLine($"Результат: {Result}");
                Console.WriteLine($"Прошла норматив: {(HasPassed ? "Да" : "Нет")}");
            }
        }

        public class Participant100M : Participant
        {
            public Participant100M(string surname, string group, string trainer) : base(surname, group, trainer)
            {
                Norm = 12;
            }
        }

        public class Participant500M : Participant
        {
            public Participant500M(string surname, string group, string trainer) : base(surname, group, trainer)
            {
                Norm = 90;
            }
        }
    }
}
