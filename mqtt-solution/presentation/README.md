# Presentation (Flutter App)

This is the Flutter frontend for the MQTT meter application. It connects to the backend API to display real-time MQTT data and readings.

## Folder Structure

```
lib/
    presentation/   - UI screens, widgets, and user interface components
        bloc/       - State management (BLoC pattern)
        pages/      - App screens and page layouts
        widgets/    - Reusable UI components
    core/          - Shared utilities, constants, and common functionality
        constants/  - App-wide constants and configuration values
        errors/     - Error handling and custom exceptions
        utils/      - Helper functions and utilities
    data/          - API calls, data sources, and data models
        datasources/ - Remote and local data sources
        models/     - Data transfer objects and JSON models
        repositories/ - Implementation of repository interfaces
    domain/        - Business logic, entities, and use cases
        entities/   - Core business objects
        repositories/ - Repository interfaces
        usecases/   - Business logic and use cases
    config/        - App configuration and settings
        routes/     - App navigation and routing
        theme/      - App theme and styling
    main.dart      - App entry point
```
