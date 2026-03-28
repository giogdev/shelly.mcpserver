# schema-validator

Validate a JSON or JSONC file against a JSON Schema using the project's Python validator.

## When to use

TRIGGER when: the user asks to validate, verify, or check a JSON file against a schema, or asks if a JSON conforms/matches a schema. Also trigger when the user mentions "schema validation", "schema check", or asks to validate an API response.

## Tool

The validator script is located at `tools/schema_validor.py`.
It accepts two positional arguments:

```
python tools/schema_validor.py <schema_path> <data_path>
```

- `<schema_path>`: path to the JSON Schema file
- `<data_path>`: path to the JSON or JSONC file to validate

## Known schemas

| API endpoint             | Schema file                                    |
|--------------------------|------------------------------------------------|
| GET `/api/devices/get`   | `docs/schemas/api-v2-devices-get.schema.json`  |

When the user asks to validate a response from one of these endpoints, use the corresponding schema automatically without asking.

For schemas not listed above, check `docs/schemas/` for a matching `.schema.json` file. If none is found, ask the user which schema to use.

## How to execute

1. Identify the **schema** path (from the table above or from the user).
2. Identify the **data** file path (from the user).
3. Run via the Bash tool:
   ```
   python tools/schema_validor.py "<schema_path>" "<data_path>"
   ```
4. Interpret the result:
   - **Exit 0** — `VALID`: confirm to the user that the file matches the schema.
   - **Exit 2** — `INVALID`: show the user the error list (paths + messages) and suggest fixes if possible.
   - **Exit 3** — a file was not found; tell the user which one is missing.
   - **Exit 4** — a file could not be parsed; tell the user which file has syntax errors.
5. If `jsonschema` is not installed the script auto-installs it. If that fails, ask the user to run `pip install jsonschema`.
