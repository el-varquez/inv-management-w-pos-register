#!/usr/bin/env node
import { readdirSync, readFileSync, existsSync } from 'node:fs';
import { join, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('..', import.meta.url));
const lock = JSON.parse(readFileSync(join(root, 'architecture', 'dependencies.json'), 'utf8'));
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
const hits = [];

const foundCsproj = new Set();
for (const file of walk(join(root, 'src'))) {
  if (file.endsWith('.csproj')) foundCsproj.add(rel(file));
}
for (const csproj of foundCsproj) {
  if (!(csproj in lock)) {
    hits.push(`${csproj}: R1 csproj not in architecture/dependencies.json — add it there in the same PR`);
  }
}
for (const [csproj, expected] of Object.entries(lock)) {
  if (!foundCsproj.has(csproj)) {
    hits.push(`${csproj}: R1 listed in architecture/dependencies.json but missing from the repo`);
    continue;
  }
  const xml = readFileSync(join(root, csproj), 'utf8');
  const projects = [...xml.matchAll(/<ProjectReference\s+Include="([^"]+)"/g)]
    .map(([, path]) => path.split(/[\\/]/).pop().replace(/\.csproj$/, ''));
  const packages = [...xml.matchAll(/<PackageReference\s+Include="([^"]+)"/g)].map(([, name]) => name);
  for (const [kind, actual, wanted] of [
    ['project', projects, expected.projects],
    ['package', packages, expected.packages],
  ]) {
    for (const name of wanted) {
      if (!actual.includes(name)) {
        hits.push(`${csproj}: R1 missing ${kind} reference "${name}" (in the lock, not the csproj)`);
      }
    }
    for (const name of actual) {
      if (!wanted.includes(name)) {
        hits.push(`${csproj}: R1 extra ${kind} reference "${name}" — if deliberate, add it to architecture/dependencies.json in the same PR`);
      }
    }
  }
}

const features = join(root, 'src', 'POS.Register', 'Features');
const LAYERS = new Set(['Screens', 'Components', 'Services']);
if (existsSync(features)) {
  for (const file of walk(features)) {
    const parts = relative(features, file).split(sep);
    if (parts.length < 3 || !LAYERS.has(parts[1])) {
      hits.push(`${rel(file)}: R2 breaks the slice shape — Features/<Feature>/{Screens|Components|Services}/<file>`);
    }
  }
  for (const file of walk(features)) {
    if (!/\.(cs|xaml)$/.test(file)) continue;
    const own = relative(features, file).split(sep)[0];
    const lines = readFileSync(file, 'utf8').split(/\r?\n/);
    lines.forEach((text, index) => {
      const match = text.match(/POS\.Register\.Features\.(\w+)/);
      if (match && match[1] !== own) {
        hits.push(`${rel(file)}:${index + 1}: R4 reaches into Features.${match[1]} from Features.${own}`);
      }
    });
  }
}

for (const file of walk(join(root, 'src'))) {
  if (!/\.(cs|xaml)$/.test(file)) continue;
  if (rel(file).split('/').includes('Services')) continue;
  const lines = readFileSync(file, 'utf8').split(/\r?\n/);
  lines.forEach((text, index) => {
    if (/\bApiClient\b/.test(text)) {
      hits.push(`${rel(file)}:${index + 1}: R3 references ApiClient outside a Services/ folder`);
    }
  });
}

if (hits.length > 0) {
  console.error('check:architecture FAILED — register architecture rules violated\n');
  for (const hit of hits) console.error(hit);
  process.exit(1);
}
console.log('check:architecture OK — dependency lock, slice shape, api containment, feature isolation all hold');
