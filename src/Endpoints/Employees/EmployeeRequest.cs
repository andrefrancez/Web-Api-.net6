﻿using Microsoft.VisualBasic;

namespace WebApiUdemy.Endpoints.Employees;

public record EmployeeRequest(string Email, string Password, string Name, string EmployeeCode);