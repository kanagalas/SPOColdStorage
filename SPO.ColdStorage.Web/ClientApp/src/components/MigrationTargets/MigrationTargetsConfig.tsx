import '../NavMenu.css';
import React from 'react';
import { NewTargetForm } from './NewTargetForm'
import Button from '@mui/material/Button';

interface TargetMigrationSite {
  rootURL: string;
}

export const MigrationTargetsConfig: React.FC<{ token: string }> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [targetMigrationSites, setTargetMigrationSites] = React.useState<Array<string>>([]);

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

      // Load storage config first
      getMigrationTargets(props.token)
        .then((allTargetSites: TargetMigrationSite[]) => {

          let siteUrls: string[] = [];
          allTargetSites.forEach(site => {
            siteUrls.push(site.rootURL);
          });

          setTargetMigrationSites(siteUrls);

        });
    }
  }, [props, getMigrationTargets]);

  const addNewSiteUrl = (newSiteUrl: string) => {
    if (targetMigrationSites?.includes(newSiteUrl)) {
      alert('Already have that site');
    }
    else
      setTargetMigrationSites(s => [...s, newSiteUrl]);
  };

  const removeSiteUrl = (siteUrl: string) => {
    const idx = targetMigrationSites.indexOf(siteUrl);
    if (idx > -1) {
      targetMigrationSites.splice(idx);
      setTargetMigrationSites(s => s.filter((value, i) => i !== idx));
    }
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
      else
      {
        alert(await response.text());
      }
      setLoading(false);

      })
      .catch(err => {

        // alert('Loading storage data failed');
        setLoading(false);
      });
  };

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
                <div>
                  {targetMigrationSites?.map((targetMigrationSite: string) => {
                    return <div>
                      <span>{targetMigrationSite}</span>
                      <span><Button onClick={() => removeSiteUrl(targetMigrationSite)}>Remove</Button></span>
                    </div>
                  })}

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
    </div>
  );
};
