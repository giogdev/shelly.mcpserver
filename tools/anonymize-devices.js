#!/usr/bin/env node
/**
 * Anonymizes sensitive data from Shelly v2/devices/get API response.
 * Replaces MAC addresses, IPs, coordinates, JWT keys, BSSIDs, device names, etc.
 */

const fs = require('fs');
const path = require('path');

const inputFile = path.resolve(__dirname, '..', 'docs', 'response-examples', 'v2-devices-get.json');
const outputFile = path.resolve(__dirname, '..', 'docs', 'response-examples', 'v2-devices-get.example.json');

const data = JSON.parse(fs.readFileSync(inputFile, 'utf8'));

// --- Mapping tables for consistent replacement ---
const macMap = new Map();
let macCounter = 1;

function anonymizeMac(mac) {
    if (!mac) return mac;
    const key = String(mac).toUpperCase();
    if (!macMap.has(key)) {
        const hex = macCounter.toString(16).padStart(12, '0').toUpperCase();
        macMap.set(key, hex);
        macCounter++;
    }
    return macMap.get(key);
}

const ipMap = new Map();
let ipCounter = 100;

function anonymizeIp(ip) {
    if (!ip || typeof ip !== 'string') return ip;
    if (!ip.match(/^\d+\.\d+\.\d+\.\d+/)) return ip;
    const isMqttPort = ip.includes(':');
    const port = isMqttPort ? ':' + ip.split(':')[1] : '';
    return '192.168.1.000' + port;
}

const nameMap = {
    'Porta Garage': 'Device 1 - Garage',
    'Ventola taverna': 'Device 2 - Fan',
    'Luce cucina': 'Device 3 - Kitchen Light',
    'Luci scale alte': 'Device 4 - Staircase Light',
    'Luce esterna ingresso 1 Piano': 'Device 5 - External Light',
};

let deviceNameCounter = 6;
function anonymizeName(name) {
    if (!name || typeof name !== 'string') return name;
    if (nameMap[name]) return nameMap[name];
    const anon = `Device ${deviceNameCounter}`;
    nameMap[name] = anon;
    deviceNameCounter++;
    return anon;
}

function anonymizeHostname(hostname) {
    if (!hostname || typeof hostname !== 'string') return hostname;
    // Replace all MAC-like hex sequences (6-12 hex chars) anywhere in the string
    return hostname.replace(/([0-9a-fA-F]{6,12})/gi, (match) => {
        // Only replace if it looks like a MAC (not a short common word)
        if (match.match(/^[0-9a-fA-F]{6,12}$/i) && match.match(/[0-9]/)) {
            return anonymizeMac(match);
        }
        return match;
    });
}

function anonymizeBssid(bssid) {
    if (!bssid || typeof bssid !== 'string') return bssid;
    if (!bssid.match(/^[0-9a-f]{2}(:[0-9a-f]{2}){5}$/i)) return bssid;
    const clean = bssid.replace(/:/g, '').toUpperCase();
    const anon = anonymizeMac(clean);
    return anon.replace(/(.{2})/g, '$1:').slice(0, 17).toLowerCase();
}

function anonymizeJwt(key) {
    if (!key || typeof key !== 'string') return key;
    if (key.startsWith('eyJ')) return '<redacted-jwt-key>';
    return key;
}

// --- Recursive anonymizer ---
function anonymizeObject(obj, parentKey) {
    if (obj === null || obj === undefined) return obj;
    if (Array.isArray(obj)) {
        return obj.map((item, i) => anonymizeObject(item, parentKey));
    }
    if (typeof obj !== 'object') return obj;

    const result = {};
    for (const [key, value] of Object.entries(obj)) {
        result[key] = anonymizeValue(key, value, obj);
    }
    return result;
}

function anonymizeValue(key, value, parent) {
    // MAC addresses
    if (key === 'mac') {
        if (typeof value === 'number') {
            return parseInt(anonymizeMac(String(value)), 16) || anonymizeMac(String(value));
        }
        return anonymizeMac(value);
    }

    // Device ID (top-level, often same as MAC)
    if (key === 'id' && parent && (parent.type === 'relay' || parent.type === 'sensor' || parent.type === 'inputs_reader') && parent.gen) {
        return anonymizeMac(value);
    }

    // IP addresses
    if (key === 'ip' || key === 'sta_ip') {
        return anonymizeIp(value);
    }

    // BSSID
    if (key === 'bssid') {
        return anonymizeBssid(value);
    }

    // WiFi SSID - anonymize AP SSIDs that contain MAC
    if (key === 'ssid' && typeof value === 'string') {
        if (/[0-9a-fA-F]{6,12}/.test(value)) {
            return anonymizeHostname(value);
        }
        return value;
    }

    // Hostname or "device" field with MAC-like content
    if ((key === 'hostname' || key === 'device') && typeof value === 'string') {
        if (/[0-9a-fA-F]{6,12}/.test(value)) {
            return anonymizeHostname(value);
        }
        return value;
    }

    // Geographic coordinates
    if (key === 'lat') return 0.0;
    if (key === 'lng' || key === 'lon') return 0.0;

    // Timezone & country
    if (key === 'timezone' || key === 'tz') return typeof value === 'string' ? 'Europe/London' : value;
    if (key === '_gip_c') return 'GB';

    // Device name - anonymize all string "name" fields
    if (key === 'name' && typeof value === 'string') {
        // If it contains a MAC-like pattern, treat as hostname
        if (value.match(/[0-9a-fA-F]{6,12}/)) {
            return anonymizeHostname(value);
        }
        return anonymizeName(value);
    }

    // Username
    if (key === 'username' && typeof value === 'string') return 'your-username';

    // JWT keys
    if (key === 'key' && typeof value === 'string' && value.startsWith('eyJ')) {
        return anonymizeJwt(value);
    }

    // MQTT server
    if (key === 'server' && typeof value === 'string' && value.match(/^\d+\.\d+\.\d+\.\d+/)) {
        return anonymizeIp(value);
    }

    // MQTT id, client_id, topic_prefix - contain MAC or device info
    if ((key === 'id' || key === 'client_id' || key === 'topic_prefix') && typeof value === 'string' && parent) {
        // Check if inside mqtt settings
        if (parent.reconnect_timeout_max !== undefined || parent.enable !== undefined || parent.server !== undefined) {
            // May contain MAC or custom name - anonymize both parts
            if (/[0-9a-fA-F]{6,12}/.test(value)) {
                return anonymizeHostname(value);
            }
            return anonymizeName(value);
        }
        // G2 settings id field
        if (parent.mqtt !== undefined && parent.wifi !== undefined && parent.sys !== undefined) {
            if (/[0-9a-fA-F]{6,12}/.test(value)) {
                return anonymizeMac(value);
            }
            return anonymizeName(value);
        }
    }

    // Any "id" field containing a MAC-like hex string in nested objects
    if (key === 'id' && typeof value === 'string' && /[0-9a-fA-F]{6,12}/.test(value)) {
        return anonymizeHostname(value);
    }

    // BLE reporter IDs
    if (key === 'reporter' && typeof value === 'object' && value !== null) {
        return anonymizeObject(value, key);
    }

    // Recursive
    if (typeof value === 'object') {
        return anonymizeObject(value, key);
    }

    return value;
}

// Process each device
const anonymized = data.map(device => {
    // First pass: register the device ID as a MAC
    if (device.id) {
        anonymizeMac(device.id);
    }
    return anonymizeObject(device, null);
});

// Second pass: ensure all cross-references are consistent
// Re-serialize and replace any remaining raw MACs
let json = JSON.stringify(anonymized, null, 4);

// Write output
fs.writeFileSync(outputFile, json, 'utf8');
console.log(`Anonymized ${data.length} devices -> ${outputFile}`);
console.log(`MAC mappings: ${macMap.size}`);
console.log(`IP mappings: ${ipMap.size}`);
console.log(`Name mappings: ${Object.keys(nameMap).length}`);
