from pulp import LpMinimize, LpProblem, LpVariable, lpSum, value,PULP_CBC_CMD

def solve_ilp(objective_coeffs, constraint_coeffs, rhs_vector):
    """
    Решает целочисленную задачу линейного программирования.

    Параметры:
    - objective_coeffs: список коэффициентов целевой функции.
    - constraint_coeffs: матрица коэффициентов ограничений (список списков).
    - rhs_vector: список правых частей ограничений.

    Возвращает:
    - Статус решения.
    - Словарь с оптимальными значениями переменных.
    - Значение целевой функции.
    """
    # Проверка согласованности данных
    if len(constraint_coeffs) != len(rhs_vector):
       raise ValueError("Количество строк в матрице ограничений должно совпадать с размером вектора правых частей.")


    num_vars = len(objective_coeffs)
    if any(len(row) != num_vars for row in constraint_coeffs):
        raise ValueError("Все строки в матрице ограничений должны иметь ту же длину, что и вектор целевой функции.")

    # Создание задачи
    problem = LpProblem("Integer_Linear_Programming", LpMinimize)

    # Создание переменных
    variables = [LpVariable(f"x{i+1}", lowBound=0, cat="Integer") for i in range(num_vars)]

    # Задание целевой функции
    problem += lpSum(objective_coeffs[i] * variables[i] for i in range(num_vars)), "Objective"

    # Добавление ограничений
    for i, (coeffs, rhs) in enumerate(zip(constraint_coeffs, rhs_vector)):
        problem += lpSum(coeffs[j] * variables[j] for j in range(num_vars)) <= rhs, f"Constraint_{i+1}"

    # Решение задачи
    solver = PULP_CBC_CMD(msg=False)

    status = problem.solve(solver)

    # Сбор результатов
    result = {
        "success": problem.status,
        "variables": [var.varValue for var in variables],
        "objective_value": value(problem.objective),
    }

    return result