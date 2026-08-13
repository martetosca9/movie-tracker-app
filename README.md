# CineTracker

App web para llevar el control de películas que viste, estás viendo o querés ver. Podés buscar en [TMDB](https://www.themoviedb.org/), importarlas a tu lista local, marcar estado y puntuarlas.

Stack: **ASP.NET Core** (minimal APIs) + **EF Core / SQLite** + frontend estático en `wwwroot`.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Cómo correrla

```bash
dotnet restore
dotnet run
```

Abrí [http://localhost:5255](http://localhost:5255).

La base SQLite (`movies.db`) se crea sola al arrancar, con algunas películas de ejemplo.

## Configurar TMDB (opcional)

Sin API key la app funciona en **modo demo** (pocas películas hardcodeadas).

1. Pedí una API key gratis en [themoviedb.org/settings/api](https://www.themoviedb.org/settings/api)
2. Configurala de una de estas formas:

**Opción A — variable de entorno**

```bash
export TMDB_API_KEY=tu_api_key
dotnet run
```

**Opción B — `appsettings.Development.json`**

```json
{
  "Tmdb": {
    "ApiKey": "tu_api_key"
  }
}
```

No subas tu key real al repo. Preferí la variable de entorno o un archivo local ignorado por git.

## Qué podés hacer

- Buscar películas en TMDB y ver populares
- Importarlas a tu lista (con póster, sinopsis, géneros y director)
- Evitar duplicados (mismo `TmdbId` no se importa dos veces)
- Filtrar por estado: Por ver / Viendo / Vista / Abandonada
- Puntuar (1–10) y editar o eliminar
- Agregar películas manualmente (sin TMDB)

## API

Base: `/api/movies`

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/movies` | Listar (query: `search`, `status`, `genre`) |
| `GET` | `/api/movies/{id}` | Obtener por id |
| `POST` | `/api/movies` | Crear manual |
| `PUT` | `/api/movies/{id}` | Actualizar |
| `DELETE` | `/api/movies/{id}` | Eliminar |
| `GET` | `/api/movies/external/search?query=` | Buscar en TMDB |
| `GET` | `/api/movies/external/popular` | Populares en TMDB |
| `POST` | `/api/movies/import` | Importar desde TMDB |

### Importar desde TMDB

```json
{
  "tmdbId": 27205,
  "status": 2,
  "rating": 9
}
```

`status`: `0` Por ver · `1` Viendo · `2` Vista · `3` Abandonada

Si la película ya está en tu lista, responde `409 Conflict`.

En Development también está el OpenAPI en `/openapi/v1.json`.

## Estructura

```
├── Data/              # DbContext + seed
├── Endpoints/         # Minimal APIs de películas
├── Models/            # Movie, WatchStatus
├── Services/          # Cliente TMDB
├── wwwroot/           # UI (index.html)
├── Program.cs
└── appsettings*.json
```

## Notas

- Los artefactos de build (`bin/`, `obj/`) y `movies.db` están en `.gitignore`
- Cambios de schema en SQLite se aplican al arranque (columna `TmdbId` + índice único)
