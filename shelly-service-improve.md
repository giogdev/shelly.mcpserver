# ShellyCloudService - Refactoring Checklist

File target: `Shelly.Services/Services/ShellyCloudService.cs`

Nota: il punto 1 (auth key in URL) e' escluso da questa checklist come richiesto.

## Verifica test esistenti (Shelly.Test)

- [x] Esiste una suite dedicata a `ShellyCloudService`:
  - `Shelly.Test/Services/ShellyCloudServiceTests.cs`
- [x] Esiste una suite dedicata al mapper usato dal service:
  - `Shelly.Test/Mapper/ShellyCloudMapperTests.cs`
- [x] Esecuzione test locale completata: **33/33 passati**
- [x] Copertura gia' presente per `FetchAndPopulateDevicesAsync`:
  - popolamento store
  - URL e payload inviati
  - empty response
  - error handling HTTP
  - mapping nomi device su risposta ridotta
- [x] Copertura gia' presente per il mapping (`ShellyCloudMapper`):
  - switch/relay/door-window
  - fallback names
  - mapping G1/G2 in `MapDevicesToStoreItems`

## Piano Refactoring (punti 2-10)

### 2) JsonSerializerOptions ricreate ogni volta (Performance)
- [x] Introdurre campi static readonly per serializzazione/deserializzazione
- [x] Riutilizzare le options nei metodi privati di (de)serializzazione
- [x] Verificare che i test esistenti restino verdi

### 3) Metodi con naming privato ma visibilita' incoerente (Encapsulation)
- [x] Verificare visibilita' reale di `_serializeAndPreparePayloadForHttpRequest`
- [x] Verificare visibilita' reale di `_deserializeApiResponseAsync`
- [x] Allineare naming/visibilita' (`private` se utility interna) - gia' conformi

### 4) Console.WriteLine invece di ILogger (Observability)
- [x] Iniettare `ILogger<ShellyCloudService>` nel costruttore
- [x] Sostituire `Console.WriteLine` con `LogWarning`/`LogInformation`
- [x] Mantenere i messaggi con contesto strutturato (count/status)

### 5) Eccezioni generiche (Error handling)
- [x] Sostituire `Exception` nel costruttore con eccezioni specifiche
- [x] Usare messaggi chiari per chiavi config mancanti
- [x] Valutare eccezione custom solo se serve nel dominio (non necessaria in questa fase)

### 6) Switch su magic strings (Manutenibilita')
- [x] Estrarre codici device in costanti centralizzate
- [x] Valutare dictionary strategy per mapper per codice
- [x] Preservare comportamento di default/fallback

### 7) Typo nel nome metodo
- [x] Rinominare `GeKnownDevices` in `GetKnownDevices`
- [x] Cercare e aggiornare tutte le chiamate/contratti (interfacce incluse)
- [x] Valutare compatibilita' backward (wrapper `[Obsolete]` se necessario)

### 8) `_tryGetStringProperty` inutilizzato
- [x] Cercare riferimenti in tutta la solution
- [x] Rimuovere metodo se non usato (rimosso nel service; nel mapper e' usato)
- [x] Rieseguire test per escludere regressioni

### 9) Duplicazione logica HTTP
- [x] Estrarre helper generico tipo `PostApiAsync<TRequest, TResponse>`
- [x] Centralizzare retry + status check + (de)serializzazione
- [x] Snellire i metodi pubblici (`GetDeviceStateAsync`, `FetchAndPopulateDevicesAsync`, `ControlSwitchDevice`)

### 10) Naming parametri costruttore
- [x] Rinominare `_config`, `_clientFactory`, `_store` in `config`, `clientFactory`, `store`
- [x] Allineare convenzioni C# (underscore solo campi privati)

## Gap di test da aggiungere durante il refactor

- [x] Test su `GetDeviceStateAsync` per codici device e canali differenti
- [ ] Test su `ControlSwitchDevice` (success/error + payload)
- [ ] Test su nuove utility HTTP estratte (happy path + errore)
- [ ] Test su configurazione mancante (eccezioni specifiche)
- [ ] Test su rename `GetKnownDevices` e impatti su contratto
