using System.Reflection;
using System.Text.Json;

namespace Lab8Test.Green
{
    [TestClass]
    public sealed class Task5
    {
        record InputStudent(string Name, string Surname, int[] Marks);
        record InputGroup(string Name, InputStudent[] Students);
        record OutputGroup(string Name, double AverageMark);

        private InputGroup[] _inputGroups;
        private OutputGroup[] _outputBase;
        private OutputGroup[] _outputElite;
        private OutputGroup[] _outputSpecial;

        private Lab8.Green.Task5.Student[] _students;
        private Lab8.Green.Task5.Group[] _groups;

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

            _inputGroups = input.GetProperty("Task5").Deserialize<InputGroup[]>();

            _outputBase = output.GetProperty("Task5").GetProperty("Base").Deserialize<OutputGroup[]>();
            _outputElite = output.GetProperty("Task5").GetProperty("Elite").Deserialize<OutputGroup[]>();
            _outputSpecial = output.GetProperty("Task5").GetProperty("Special").Deserialize<OutputGroup[]>();

            _students = _inputGroups
                .SelectMany(g => g.Students)
                .Select(s => new Lab8.Green.Task5.Student(s.Name, s.Surname))
                .ToArray();
        }

        [TestMethod]
        public void Test_00_OOP()
        {
            var student = typeof(Lab8.Green.Task5.Student);

            Assert.IsTrue(student.IsValueType);
            Assert.AreEqual(0, student.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);

            Assert.IsTrue(student.GetProperty("Name")?.CanRead ?? false);
            Assert.IsTrue(student.GetProperty("Surname")?.CanRead ?? false);
            Assert.IsTrue(student.GetProperty("Marks")?.CanRead ?? false);
            Assert.IsTrue(student.GetProperty("AverageMark")?.CanRead ?? false);

            Assert.IsFalse(student.GetProperty("Name")?.CanWrite ?? true);
            Assert.IsFalse(student.GetProperty("Surname")?.CanWrite ?? true);
            Assert.IsFalse(student.GetProperty("Marks")?.CanWrite ?? true);
            Assert.IsFalse(student.GetProperty("AverageMark")?.CanWrite ?? true);

            Assert.IsNotNull(student.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(string) },
                null));

            Assert.IsNotNull(student.GetMethod("Exam"));
            Assert.IsNotNull(student.GetMethod("Print"));

            Assert.AreEqual(0, student.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.AreEqual(student.GetConstructors().Count(c => c.IsPublic), 1);
            Assert.AreEqual(student.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 10);
            Assert.AreEqual(student.GetProperties().Count(p => p.PropertyType.IsPublic), 4);

            var group = typeof(Lab8.Green.Task5.Group);

            Assert.IsTrue(group.IsClass);
            Assert.AreEqual(0, group.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);

            Assert.IsTrue(group.GetProperty("Name")?.CanRead ?? false);
            Assert.IsTrue(group.GetProperty("Students")?.CanRead ?? false);
            Assert.IsTrue(group.GetProperty("AverageMark")?.CanRead ?? false);

            Assert.IsTrue(group.GetProperty("AverageMark")!.GetGetMethod()!.IsVirtual);

            Assert.IsNotNull(group.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(string) },
                null));

            Assert.IsNotNull(group.GetMethod("Add", new[] { typeof(Lab8.Green.Task5.Student) }));
            Assert.IsNotNull(group.GetMethod("Add", new[] { typeof(Lab8.Green.Task5.Student[]) }));
            Assert.IsNotNull(group.GetMethod("Print"));
            Assert.IsNotNull(group.GetMethod("SortByAverageMark"));

            Assert.AreEqual(0, group.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.AreEqual(group.GetConstructors().Count(c => c.IsPublic), 1);
            Assert.AreEqual(group.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 10);
            Assert.AreEqual(group.GetProperties().Count(p => p.PropertyType.IsPublic), 3);

            Assert.IsTrue(typeof(Lab8.Green.Task5.EliteGroup).IsSubclassOf(group));
            Assert.IsTrue(typeof(Lab8.Green.Task5.SpecialGroup).IsSubclassOf(group));

            group = typeof(Lab8.Green.Task5.EliteGroup);
            Assert.AreEqual(0, group.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.AreEqual(group.GetConstructors().Count(c => c.IsPublic), 1);
            Assert.AreEqual(group.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 10);
            Assert.AreEqual(group.GetProperties().Count(p => p.PropertyType.IsPublic), 3);

            group = typeof(Lab8.Green.Task5.SpecialGroup);
            Assert.AreEqual(0, group.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.AreEqual(group.GetConstructors().Count(c => c.IsPublic), 1);
            Assert.AreEqual(group.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 10);
            Assert.AreEqual(group.GetProperties().Count(p => p.PropertyType.IsPublic), 3);

        }

        [TestMethod]
        public void Test_01_CreateStudents()
        {
            Assert.AreEqual(
                _inputGroups.Sum(g => g.Students.Length),
                _students.Length);
        }

        [TestMethod]
        public void Test_02_Exams()
        {
            RunExams();

            int idx = 0;
            foreach (var g in _inputGroups)
            {
                foreach (var s in g.Students)
                {
                    var marks = s.Marks.Where(m => m > 0).ToArray();
                    double avg = marks.Length == 0 ? 0 : marks.Average();

                    Assert.AreEqual(avg, _students[idx].AverageMark, 0.0001);
                    idx++;
                }
            }
        }

        [TestMethod]
        public void Test_03_BaseGroup_Average()
        {
            RunExams();
            InitGroups<BaseGroupFactory>();

            CheckGroups(_outputBase);
        }

        [TestMethod]
        public void Test_04_EliteGroup_Average()
        {
            RunExams();
            InitGroups<EliteGroupFactory>();

            CheckGroups(_outputElite);
        }

        [TestMethod]
        public void Test_05_SpecialGroup_Average()
        {
            RunExams();
            InitGroups<SpecialGroupFactory>();

            CheckGroups(_outputSpecial);
        }

        [TestMethod]
        public void Test_06_Sort()
        {
            RunExams();
            InitGroups<EliteGroupFactory>();

            Lab8.Green.Task5.Group.SortByAverageMark(_groups);

            for (int i = 0; i < _groups.Length - 1; i++)
            {
                Assert.IsTrue(_groups[i].AverageMark >= _groups[i + 1].AverageMark);
            }
        }

        private void RunExams()
        {
            int idx = 0;
            foreach (var g in _inputGroups)
            {
                foreach (var s in g.Students)
                {
                    foreach (var m in s.Marks)
                        _students[idx].Exam(m);
                    idx++;
                }
            }
        }

        private void ResetAllParticipantStatics()
        {
            var baseType = typeof(Lab8.Green.Task5.Group);

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
        private void InitGroups<T>() where T : IGroupFactory, new()
        {
            ResetAllParticipantStatics();
            var factory = new T();

            _groups = _inputGroups
                .Select(g => factory.Create(g.Name))
                .ToArray();

            for (int i = 0, k = 0; i < _groups.Length; i++)
            {
                for (int j = 0; j < _inputGroups[i].Students.Length; j++)
                {
                    _groups[i].Add(_students[k++]);
                }
            }
        }

        private void CheckGroups(OutputGroup[] expected)
        {
            for (int i = 0; i < _groups.Length; i++)
            {
                Assert.AreEqual(expected[i].Name, _groups[i].Name);
                Assert.AreEqual(expected[i].AverageMark, _groups[i].AverageMark, 0.0001);
            }
        }

        interface IGroupFactory
        {
            Lab8.Green.Task5.Group Create(string name);
        }

        class BaseGroupFactory : IGroupFactory
        {
            public Lab8.Green.Task5.Group Create(string name) =>
                new Lab8.Green.Task5.Group(name);
        }

        class EliteGroupFactory : IGroupFactory
        {
            public Lab8.Green.Task5.Group Create(string name) =>
                new Lab8.Green.Task5.EliteGroup(name);
        }

        class SpecialGroupFactory : IGroupFactory
        {
            public Lab8.Green.Task5.Group Create(string name) =>
                new Lab8.Green.Task5.SpecialGroup(name);
        }
    }
}
