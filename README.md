# \# FPV Configurator

# 

# Веб-конфігуратор FPV-дрона за принципом конфігуратора ПК: користувач обирає раму, польотний контролер, ESC, мотори, пропелери, камеру, відеопередавач і батарею. Система перевіряє сумісність компонентів (розмір кріплення стека, діаметр пропелерів, кількість банок батареї тощо) і рахує загальну ціну та вагу збірки.

# Репозиторій захищено: зміни в main лише через Pull Request.
 
# 

# \*\*Стек:\*\* .NET 10, C#, Blazor, xUnit + Moq.

# 

# \## Структура рішення

# 

# | Проєкт | Призначення |

# |---|---|

# | FpvConfigurator.Core | Інтерфейси та перелічення (IComponent, ComponentType) |

# | FpvConfigurator.Components | Класи компонентів |

# | FpvConfigurator.ManualCheck | Консольна ручна перевірка класів |

# 

# \## Команда і розподіл робіт

# 

# | Роль | GitHub | Що реалізує |

# |---|---|---|

# | Тімлід | <логін-тімліда> | Репозиторій, Core: IComponent, ComponentType; рев'ю і злиття PR |

# | Розробник | SilentProg | ComponentBase, Frame, Motor, Propeller, Battery (ЛР1); FlightController, Esc, Camera, Vtx (ЛР2) |

# | Тестувальник | <логін-тестувальника> | ManualCheck, тест-кейси, Issues про дефекти; модульні тести (ЛР2+) |

# 

# \## Гілки

# 

# \- `main` – захищена, зміни тільки через Pull Request з approve

# \- `feature/core-interfaces` – тімлід

# \- `feature/components` – розробник

# \- `test/manual-check` – тестувальник

