import '../NavMenu.css';
import React from 'react';

interface TargetMigrationSite {
  rootURL: string;
}

export const MigrationTargetsConfig : React.FC<{token:string}> = (props) => {

  const [loading, setLoading] = React.useState<boolean>(false);
  const [targetMigrationSites, setTargetMigrationSites] = React.useState<TargetMigrationSite[] | null>(null);

  const getMigrationTargets = React.useCallback(async (token) => 
  {
    return await fetch('migration', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token,
      }}
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
        
        setTargetMigrationSites(allTargetSites);

      });
    }
  }, [targetMigrationSites, props]);

  
    return (
      <div>
        <h1>Cold Storage Access Web</h1>

        <p>Target sites for migration.</p>
          
          {!loading ?
            (
              <table>
                {targetMigrationSites?.map((targetMigrationSite: TargetMigrationSite) => 
                {
                  return <tr>
                    <td>{targetMigrationSite.rootURL}</td>
                  </tr>
                })}
                <tr>
                  <td></td>
                </tr>
              </table>
            )
            : <div>Loading</div>
          }
      </div>
    );
};
