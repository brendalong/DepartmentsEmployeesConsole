using DepartmentsEmployeesConsole.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DepartmentsEmployeesConsole.Data
{
    //this class gets data from DB
    public class EmployeeRepository
    {
        //method for connecting
        public SqlConnection Connection
        {
            get
            {
                string _connectionString = "Data Source=localhost\\SQLEXPRESS; Initial Catalog=DepartmentsEmployees; Integrated Security=True";
                return new SqlConnection(_connectionString);
            }

        }

        public List<Employee> GetAllEmployees()
        {
            //1. open db connection
            //2. create SQL select statement as C# string
            //3. execute SQL statement against DB
            //4. From DB, get 'raw data'. Parse into c# object
            //5. close connection
            //6. return object

            //setting up var for initial list that will be returned
            List<Employee> allEmployees = new List<Employee>();

            //open connection SQLConnection is the TUNNEL
            using (SqlConnection conn = Connection)
            {
                //open 
                conn.Open();

                //list of instructions to give truck driver when reach end of tunnel(db)
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //define the sql command to run when truck driver gets to db
                    cmd.CommandText = @"
                        SELECT e.id, e.FirstName, e.LastName, e.DepartmentId, d.id, d.DeptName
                        FROM Employee e
                        LEFT JOIN Department d
                        ON e.DepartmentId = d.Id";

                    //ExecuteReader has the driver goto db and execute the command
                    //the driver then comes back with a truckload of data from DB
                    //This is held in variale "reader"

                    SqlDataReader reader = cmd.ExecuteReader();

                    //we are going to use the list created earlier

                    //the reader will read the returned data one row at a time, so put it in a while loop
                    while (reader.Read())
                    {
                        //get ordinal returns what "position" the Id column is in
                        int idColumn = reader.GetOrdinal("Id");
                        int idValue = reader.GetInt32(idColumn);

                        //have to tell reader what TYPE of data it is reading `GetInt32, GetString, GetDate, etc
                        int firstNameColumn = reader.GetOrdinal("FirstName");
                        string firstNameValue = reader.GetString(firstNameColumn);

                        int lastNameColumn = reader.GetOrdinal("LastName");
                        string lastNameValue = reader.GetString(lastNameColumn);

                        int deptIdColumn = reader.GetOrdinal("DepartmentId");
                        int deptIdValue = reader.GetInt32(deptIdColumn);

                        int deptNameColumn = reader.GetOrdinal("DeptName");
                        string deptNameValue = reader.GetString(deptNameColumn);

                        //with data parsed, create C# object
                        var employee = new Employee()
                        {
                            Id = idValue,
                            FirstName = firstNameValue,
                            LastName = lastNameValue,
                            DepartmentId = deptIdValue,
                            Department = new Department()
                            {
                                Id = deptIdValue,
                                DeptName = deptNameValue
                            }
                        };

                        //add this to the list
                        allEmployees.Add(employee);
                    }

                    //close the connection
                    reader.Close();

                    //finally return the employee list
                    return allEmployees;

                }
            }
        }

        //now let's get one employee
        public Employee GetEmployeeById(int employeeId)
        {
            //setting up var for initial list that will be returned
            List<Employee> allEmployees = new List<Employee>();

            //open connection SQLConnection is the TUNNEL
            using (SqlConnection conn = Connection)
            {
                //open 
                conn.Open();

                //list of instructions to give truck driver when reach end of tunnel(db)
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //define the sql command to run when truck driver gets to db
                    cmd.CommandText = @"
                        SELECT e.id, e.FirstName, e.LastName, e.DepartmentId, d.id, d.DeptName
                        FROM Employee e
                        LEFT JOIN Department d
                        ON e.DepartmentId = d.Id
                        WHERE e.Id = @id";

                    //for safety - use sql parameter
                    cmd.Parameters.Add(new SqlParameter("@id", employeeId));

                    //ExecuteReader has the driver goto db and execute the command
                    //the driver then comes back with a truckload of data from DB
                    //This is held in variale "reader"

                    SqlDataReader reader = cmd.ExecuteReader();

                    //we are going to use the list created earlier

                    //only a single row. If not found, will return false.
                    if (reader.Read())
                    {
                        //get ordinal returns what "position" the Id column is in
                        int idColumn = reader.GetOrdinal("Id");
                        int idValue = reader.GetInt32(idColumn);

                        //have to tell reader what TYPE of data it is reading `GetInt32, GetString, GetDate, etc
                        int firstNameColumn = reader.GetOrdinal("FirstName");
                        string firstNameValue = reader.GetString(firstNameColumn);

                        int lastNameColumn = reader.GetOrdinal("LastName");
                        string lastNameValue = reader.GetString(lastNameColumn);

                        int deptIdColumn = reader.GetOrdinal("DepartmentId");
                        int deptIdValue = reader.GetInt32(deptIdColumn);

                        int deptNameColumn = reader.GetOrdinal("DeptName");
                        string deptNameValue = reader.GetString(deptNameColumn);

                        //with data parsed, create C# object
                        var employee = new Employee()
                        {
                            Id = idValue,
                            FirstName = firstNameValue,
                            LastName = lastNameValue,
                            DepartmentId = deptIdValue,
                            Department = new Department()
                            {
                                Id = deptIdValue,
                                DeptName = deptNameValue
                            }
                        };

                        //close the connection
                        reader.Close();

                        return employee;
                    }
                    else
                    {
                        //didn't find one
                        return null;
                    }
                }
            }
        }

        //create new employee

        public Employee CreateNewEmployee(Employee employeeToAdd)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();

                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    INSERT INTO Employee (FirstName, LastName, DepartmentId)
                    OUTPUT INSERTED.Id
                    VALUES (@firstName, @lastName, @departmentId)";

                    //make safety
                    cmd.Parameters.Add(new SqlParameter("@firstName", employeeToAdd.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", employeeToAdd.LastName));
                    cmd.Parameters.Add(new SqlParameter("@departmentId", employeeToAdd.DepartmentId));

                    //gets the returned id (output)
                    int id = (int)cmd.ExecuteScalar();

                    employeeToAdd.Id = id;

                    return employeeToAdd;

                }
            }
        }

        //update an employee
        public void UpdateEmployee(int employeeId, Employee employee)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();

                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    UPDATE Employee
                    SET FirstName = @firstName, LastName = @lastName, DepartmentId = @departmentId
                    WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@departmenId", employee.DepartmentId));
                    cmd.Parameters.Add(new SqlParameter("@Id", employeeId));

                    //don't expect return, so nonQuery
                    cmd.ExecuteNonQuery();

                }
            }
        }


        //delete an employee

        public void DeleteEmployee(int employeeId)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();

                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Employee WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", employeeId));
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
