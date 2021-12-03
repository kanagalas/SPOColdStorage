import React from 'react';

interface SharePointFile
{
    FileName: string;
}
interface MigrationLog
{
    File: SharePointFile
}

interface SearchLogsState
{ 
    searchLogs: Array<MigrationLog>, 
    loading: boolean,
    searchTerm: string;
}


export class FindLog extends React.Component<{}, SearchLogsState> {
    static displayName = FindLog.name;

    constructor(props : any) {
        super(props);
        this.state = { searchLogs: [], loading: true, searchTerm: "" };
    }

    componentDidMount() {
        if (this.state.searchTerm != "") {
            this.populateSearchLogsFromSearch();
        }
    }

    static renderResultsTable(logs: Array<MigrationLog>) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>File name</th>
                    </tr>
                </thead>
                <tbody>
                    {logs.map((log : MigrationLog) =>
                        <tr key={log.File?.FileName}>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : FindLog.renderResultsTable(this.state.searchLogs);

        return (
            <div>
                <h1 id="tabelLabel">Migration Logs</h1>
                <p>Search for a file migrated.</p>
                {contents}
            </div>
        );
    }

    async populateSearchLogsFromSearch() {
        await fetch('migrationrecord?keyWord=' + this.state.searchTerm)
            .then(async response => {

                const data = await response.json();
                this.setState({ searchLogs: data, loading: false });
            })
            .catch(err => {
                alert('Loading data failed');
            });
        
    }
}
