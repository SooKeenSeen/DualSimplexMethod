from scipy.optimize import linprog

def solve_lp(c, A, b, bounds):
    """
    Решает задачу линейного программирования.

    :param c: Коэффициенты целевой функции (список)
    :param A: Коэффициенты ограничений (список списков)
    :param b: Правая часть ограничений (список)
    :param bounds: Границы переменных (список кортежей)
    :return: Результат решения в виде словаря
    """
    result = linprog(c, A_ub=A, b_ub=b, bounds=bounds, method='highs')
    if result.success:
        return {
            "success": True,
            "x": result.x.tolist(),
            "objective_value": -result.fun
        }
    else:
        return {
            "success": False,
            "message": "Не удалось найти решение"
        }