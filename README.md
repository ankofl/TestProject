## Структура проекта

### Prefabs:

1. DroneAuthoring - Сущность дрона
2. HomeAuthoring - Сущность базы
3. OreAuthoring - Сущность руды (ресурса)
4. OreInDroneAuthoring - Сущность добытой руды в дроне
5. PrefabsAuthoring - Сущность хранения префабов всех сущностей

### Scripts:

1. DroneMiningOreSystem - Система добычи руды
2. DroneMoveDisableSystem - Система приоритета дронов доставляющих руду на базу, над дронами летящими к руде
3. DroneOreSelectSystem - Система выбора руды незанятой другими дронами команды
4. DroneRotationSystem - Система поворота (в данным момент стабилизации вращения) дрона
5. DroneSpawnSystem - Система спавна дронов при старте симуляции
6. DroneToHomeSystem - Система возвращения дрона с рудой на базу
7. DroneToOreReorderSystem - Система сброса цели дрона (ранее выбранной в DroneOreSelectSystem), если её ранее добыл дрон другой команды
8. DroneTrailSystem - Система отрисовки машрута дронов до выбранной руды (работает только в редакторе, т.к. является отладочной функцией)
9. OreReloadSystem - Система респавна руды
10. UISystem - Система обработки UI-ввода пользователя

### UI Toolkit

1. NewUXMLTemplate - Основаная страница UI
2. Styles - Таблица стилей
3. UI - Компонент инициализации и хранения элементов UI для доступа к ним из UISystem
