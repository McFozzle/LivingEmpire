from pathlib import Path
import csv
root=Path(__file__).resolve().parents[1]
def read(name):
    with (root/'data'/name).open(encoding='utf-8-sig', newline='') as f:
        return list(csv.DictReader(f))
factions=read('canonical_factions.csv')
influences=read('provisional_county_influence.csv')
unresolved=read('unresolved_faction_geography.csv')
print('Faction rows:',len(factions),'Influence rows:',len(influences),'Unresolved rows:',len(unresolved))
assert len(factions)==366, 'Unexpected faction count'
assert len(influences)==255, 'Unexpected influence count'
assert len(unresolved)==154, 'Unexpected unresolved count'
for name in ('LivingEmpire.cs','CanonicalData.cs'):
    p=root/'src'/name
    assert p.is_file() and p.stat().st_size>100, f'Missing {name}'
text=(root/'src'/'LivingEmpire.cs').read_text()
assert 'IHasModSaveData' in text and 'RestoreSaveData' in text
print('PASS: repository source/data checks (not Unity compilation).')
