import '../NavMenu.css';
import React from 'react';
import { NewTargetForm } from './NewTargetForm'
import { MigrationTarget } from './MigrationTarget'
import Button from '@mui/material/Button';

import { SiteBrowserDiag } from './SiteBrowserDiag';
import { TargetMigrationSite } from './TargetSitesInterfaces';

export const MigrationTargetsConfig: React.FC<{ token: string }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [targetMigrationSites, setTargetMigrationSites] = React.useState<Array<TargetMigrationSite>>([]);
  const [selectedSite, setSelectedSite] = React.useState<TargetMigrationSite | null>(null);

  const { forwardRef, useRef, useImperativeHandle } = React;
  const childRef = useRef();

  const getMigrationTargets = React.useCallback(async (token) => {
    return await fetch('migration', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token,
      }
    }
    )
      .then(async response => {
        const data: TargetMigrationSite[] = await response.json();
        return Promise.resolve(data);
      })
      .catch(err => {

        // alert('Loading storage data failed');
        setLoading(false);

        return Promise.reject();
      });
  }, []);

  React.useEffect(() => {

    if (props.token) {

      // Load sites config from API
      getMigrationTargets(props.token)
        .then((allTargetSites: TargetMigrationSite[]) => {

          setTargetMigrationSites(allTargetSites);

        });
    }
  }, [props, getMigrationTargets]);

  const addNewSiteUrl = (newSiteUrl: string) => {
    targetMigrationSites.forEach(s => {
      if (s.rootURL === newSiteUrl) {
        alert('Already have that site');
        return;
      }
    });

    const newSiteDef: TargetMigrationSite =
    {
      rootURL: newSiteUrl
    }
    setTargetMigrationSites(s => [...s, newSiteDef]);
  };

  const removeSiteUrl = (selectedSite: TargetMigrationSite) => {
    const idx = targetMigrationSites.indexOf(selectedSite);
    if (idx > -1) {
      targetMigrationSites.splice(idx);
      setTargetMigrationSites(s => s.filter((value, i) => i !== idx));
    }
  };

  const configureListsAndFolders = (selectedSite: TargetMigrationSite) => {
    setSelectedSite(selectedSite);
  };

  const saveAll = () => {
    setLoading(true);
    fetch('migration', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + props.token,
      },
      body: JSON.stringify(
        {
          TargetSites: targetMigrationSites
        })
    }
    ).then(async response => {
      if (response.ok) {
        alert('Success');
      }
      else {
        alert(await response.text());
      }
      setLoading(false);

    })
      .catch(err => {

        // alert('Loading storage data failed');
        setLoading(false);
      });
  };

  const closeDiag = () => {
    setSelectedSite(null);
  }

  return (
    <div>
      <h1>Cold Storage Access Web</h1>

      <p>Target sites for migration. When the migration tools run, these sites will be indexed &amp; copied to cold-storage.</p>

      {!loading ?
        (
          <div>
            {targetMigrationSites.length === 0 ?
              <div>No sites to migrate</div>
              :
              (
                <div id='migrationTargets'>
                  {targetMigrationSites.map((targetMigrationSite: TargetMigrationSite) => (
                    <MigrationTarget token={props.token} targetSite={targetMigrationSite}
                      removeSiteUrl={removeSiteUrl} configureListsAndFolders={configureListsAndFolders} />
                  ))}

                </div>
              )
            }
            <NewTargetForm addUrlCallback={(newSite: string) => addNewSiteUrl(newSite)} />

            {targetMigrationSites.length > 0 &&
              <Button variant="contained" onClick={() => saveAll()}>Save Changes</Button>
            }
          </div>
        )
        : <div>Loading...</div>
      }

      {selectedSite &&
        <SiteBrowserDiag token={props.token} targetSite={selectedSite} open={selectedSite !== null} onClose={closeDiag} />
      }
    </div>
  );
};
