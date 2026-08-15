#!/usr/bin/env node
import { readdirSync, readFileSync, existsSync } from 'node:fs';
import { join, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const tokens = JSON.parse(readFileSync(join(root, 'design', 'tokens.json'), 'utf8'));
const tokensXaml = join(root, 'src', 'POS.Register', 'Themes', 'Tokens.xaml');
const hits = [];

if (!existsSync(tokensXaml)) {
  hits.push('src/POS.Register/Themes/Tokens.xaml: missing — every color token lives there');
} else {
  const xaml = readFileSync(tokensXaml, 'utf8').toUpperCase();
  for (const [name, value] of Object.entries(tokens)) {
    if (!xaml.includes(value.toUpperCase())) {
      hits.push(`design/tokens.json: token "${name}" value ${value} not found in Themes/Tokens.xaml`);
    }
  }
}

const SKIP = new Set(['obj', 'bin', '.git']);
function* walk(dir) {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (SKIP.has(entry.name)) continue;
      yield* walk(join(dir, entry.name));
    } else {
      yield join(dir, entry.name);
    }
  }
}
const rel = (path) => relative(root, path).split(sep).join('/');
for (const file of walk(join(root, 'src'))) {
  if (!/\.(xaml|cs)$/.test(file) || file === tokensXaml) continue;
  const lines = readFileSync(file, 'utf8').split(/\r?\n/);
  lines.forEach((text, index) => {
    if (/#[0-9a-fA-F]{6,8}\b/.test(text)) {
      hits.push(`${rel(file)}:${index + 1}: raw color literal — use a Themes/Tokens.xaml resource`);
    }
  });
}

if (hits.length > 0) {
  console.error('check:design FAILED — design-system rules violated\n');
  for (const hit of hits) console.error(hit);
  process.exit(1);
}
console.log('check:design OK — tokens match design/tokens.json and no raw colors outside Themes/Tokens.xaml');
