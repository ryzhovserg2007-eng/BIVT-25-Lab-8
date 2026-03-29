using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;

namespace Lab8Test.Green
{
    [TestClass]
    public sealed class Task2
    {
        record InputRow(string Name, string Surname, int[] Marks);
        record OutputRow(string Name, string Surname, double AverageMark, bool IsExcellent);

        private InputRow[] _input;
        private OutputRow[] _output;
        private Lab8.Green.Task2.Student[] _students;

        [TestInitialize]
        public void LoadData()
        {
            var folder = Directory.GetParent(Directory.GetCurrentDirectory())
                .Parent.Parent.Parent.FullName;
            folder = Path.Combine(folder, "Lab8Test", "Green");

            var input = JsonSerializer.Deserialize<JsonElement>(
                File.ReadAllText(Path.Combine(folder, "input.json")))!;
            var output = JsonSerializer.Deserialize<JsonElement>(
                File.ReadAllText(Path.Combine(folder, "output.json")))!;

            _input = input.GetProperty("Task2").Deserialize<InputRow[]>()!;
            _output = output.GetProperty("Task2").Deserialize<OutputRow[]>()!;

            _students = new Lab8.Green.Task2.Student[_input.Length];
        }

        [TestMethod]
        public void Test_00_OOP()
        {
            var human = typeof(Lab8.Green.Task2.Human);
            var student = typeof(Lab8.Green.Task2.Student);

            Assert.IsTrue(human.IsClass);
            Assert.IsTrue(student.IsClass);
            Assert.IsTrue(student.IsSubclassOf(human));

            Assert.AreEqual(0, human.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.AreEqual(0, student.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);

            Assert.IsTrue(human.GetProperty("Name")?.CanRead ?? false);
            Assert.IsTrue(human.GetProperty("Surname")?.CanRead ?? false);

            Assert.IsFalse(human.GetProperty("Name")?.CanWrite ?? true);
            Assert.IsFalse(human.GetProperty("Surname")?.CanWrite ?? true);

            Assert.IsTrue(student.GetProperty("AverageMark")?.CanRead ?? false);
            Assert.IsTrue(student.GetProperty("IsExcellent")?.CanRead ?? false);
            Assert.IsTrue(student.GetProperty("Marks")?.CanRead ?? false);

            Assert.IsFalse(student.GetProperty("AverageMark")?.CanWrite ?? true);
            Assert.IsFalse(student.GetProperty("IsExcellent")?.CanWrite ?? true);
            Assert.IsFalse(student.GetProperty("Marks")?.CanWrite ?? true);

            Assert.IsNotNull(human.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(string) },
                null));


            Assert.AreEqual(0, human.GetFields().Count(f => f.IsPublic));
            Assert.AreEqual(2, human.GetProperties().Count(f => f.PropertyType.IsPublic));
            Assert.AreEqual(1, human.GetConstructors().Count(f => f.IsPublic));
            Assert.AreEqual(human.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 7);

            Assert.IsNotNull(student.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(string) },
                null));

            Assert.IsNotNull(student.GetMethod("Exam", BindingFlags.Instance | BindingFlags.Public));
            Assert.IsNotNull(student.GetMethod("SortByAverageMark", BindingFlags.Static | BindingFlags.Public));
            Assert.IsNotNull(student.GetMethod("Print", BindingFlags.Instance | BindingFlags.Public));

            Assert.AreEqual(0, student.GetFields().Count(f => f.IsPublic));
            Assert.AreEqual(6, student.GetProperties().Count(f => f.PropertyType.IsPublic));
            Assert.AreEqual(1, student.GetConstructors().Count(f => f.IsPublic));
            Assert.AreEqual(student.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 12);
        }

        [TestMethod]
        public void Test_01_Create()
        {
            Init();
        }

        [TestMethod]
        public void Test_02_Exam()
        {
            Init();
            Exam();
            CheckBase(examExpected: true);
        }

        [TestMethod]
        public void Test_03_Sort()
        {
            Init();
            Exam();
            Lab8.Green.Task2.Student.SortByAverageMark(_students);
            CheckSorted();
        }

        [TestMethod]
        public void Test_04_ArrayIsolation()
        {
            Init();
            Exam();

            for (int i = 0; i < _students.Length; i++)
            {
                var marks = _students[i].Marks;
                for (int j = 0; j < marks.Length; j++)
                    marks[j] = -100;
            }

            CheckBase(examExpected: true);
        }

        [TestMethod]
        public void Test_05_ExcellentCount()
        {
            Init();
            Exam();

            int expected = _output.Count(o => o.IsExcellent);
            Assert.AreEqual(expected, Lab8.Green.Task2.Student.ExcellentAmount);
        }

        private void ResetAllParticipantStatics()
        {
            var baseType = typeof(Lab8.Green.Task2.Human);

            var allTypes = baseType.Assembly
                .GetTypes()
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var type in allTypes)
            {
                var staticFields = type.GetFields(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                foreach (var field in staticFields)
                {
                    if (field.FieldType == typeof(int))
                        field.SetValue(null, 0);
                    else if (field.FieldType == typeof(double))
                        field.SetValue(null, 0.0);
                    else if (field.FieldType == typeof(bool))
                        field.SetValue(null, false);
                    else
                        field.SetValue(null, null);
                }
            }
        }
        private void Init()
        {
            ResetAllParticipantStatics();
            for (int i = 0; i < _input.Length; i++)
            {
                _students[i] = new Lab8.Green.Task2.Student(
                    _input[i].Name,
                    _input[i].Surname);
            }
        }

        private void Exam()
        {
            for (int i = 0; i < _input.Length; i++)
            {
                foreach (var mark in _input[i].Marks)
                    _students[i].Exam(mark);
            }
        }

        private void CheckBase(bool examExpected)
        {
            for (int i = 0; i < _students.Length; i++)
            {
                Assert.AreEqual(_input[i].Name, _students[i].Name);
                Assert.AreEqual(_input[i].Surname, _students[i].Surname);

                if (examExpected)
                {
                    Assert.AreEqual(
                        _input[i].Marks.Average(),
                        _students[i].AverageMark,
                        0.0001);

                    Assert.AreEqual(
                        _input[i].Marks.All(m => m >= 4),
                        _students[i].IsExcellent);
                }
                else
                {
                    Assert.AreEqual(0, _students[i].AverageMark, 0.0001);
                    Assert.IsFalse(_students[i].IsExcellent);
                }
            }
        }

        private void CheckSorted()
        {
            for (int i = 0; i < _students.Length; i++)
            {
                Assert.AreEqual(_output[i].Name, _students[i].Name);
                Assert.AreEqual(_output[i].Surname, _students[i].Surname);
                Assert.AreEqual(_output[i].AverageMark, _students[i].AverageMark, 0.0001);
                Assert.AreEqual(_output[i].IsExcellent, _students[i].IsExcellent);
            }
        }
    }
}
