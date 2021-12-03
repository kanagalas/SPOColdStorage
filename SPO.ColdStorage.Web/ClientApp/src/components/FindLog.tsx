import React from 'react';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';

interface SharePointFile
{
    fileName: string;
}
interface MigrationLog
{
    file: SharePointFile
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
        this.state = { searchLogs: [], loading: false, searchTerm: "" };
    }

    componentDidMount() {
        if (this.state.searchTerm !== "") {
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
                        <tr key={log.file?.fileName}>
                            <td>{log.file?.fileName}</td>
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
                <TextField id="outlined-basic" label="Search term" variant="outlined" required 
                    onChange={e => { this.setState({ searchTerm : e.target.value});}} />
                <Button variant="outlined" 
                    onClick={() => {
                        this.populateSearchLogsFromSearch();
                    }}
                >Search</Button>

                {contents}
            </div>
        );
    }

    async populateSearchLogsFromSearch() {
        if (this.state.searchTerm.length > 0) {
            this.setState({loading: true});
            await fetch('migrationrecord?keyWord=' + this.state.searchTerm)
            .then(async response => {

                const data = await response.json();
                console.log(data);
                this.setState({ searchLogs: data, loading: false });
                this.setState({loading: false});
            })
            .catch(err => {
                alert('Loading data failed');
                this.setState({ searchLogs: [], loading: false });
            });
        }
    }
}
