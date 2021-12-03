import React from 'react';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import moment from 'moment';

interface SharePointFile {
    url: string;
    web: SharePointWeb;
}
interface SharePointWeb {
    url: string;
}
interface MigrationLog {
    file: SharePointFile;
    lastModified: Date;
    migrated: Date;
}

interface SearchLogsState {
    searchLogs: Array<MigrationLog>,
    loading: boolean,
    searchTerm: string;
}


export class FindLog extends React.Component<{}, SearchLogsState> {
    static displayName = FindLog.name;

    constructor(props: any) {
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
            <div>
                {logs.length > 0 ?
                    <div>
                        <p>{logs.length} documents found.</p>
                        <table className='table table-striped' aria-labelledby="tabelLabel">
                            <thead>
                                <tr>
                                    <th>File name</th>
                                    <th>Web</th>
                                    <th>Last Modified</th>
                                    <th>Migrated</th>
                                </tr>
                            </thead>
                            <tbody>
                                {logs.map((log: MigrationLog) =>
                                    <tr key={log.file?.url}>
                                        <td>{log.file?.url.split('/').pop()}</td>
                                        <td>{log.file.web.url}</td>
                                        <td>{moment(log.lastModified).format('D-MMM-YYYY HH:mm')}</td>
                                        <td>{moment(log.migrated).format('D-MMM-YYYY HH:mm')}</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>


                    :
                    <div>No files found</div>
                }

            </div>
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
                <TextField id="outlined-basic" label="Search term" variant="standard" required
                    onChange={e => { this.setState({ searchTerm: e.target.value }); }} />
                <Button variant="outlined" size="large"
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
            this.setState({ loading: true });
            await fetch('migrationrecord?keyWord=' + this.state.searchTerm)
                .then(async response => {

                    const data = await response.json();
                    console.log(data);
                    this.setState({ searchLogs: data, loading: false });
                    this.setState({ loading: false });
                })
                .catch(err => {
                    alert('Loading data failed');
                    this.setState({ searchLogs: [], loading: false });
                });
        }
    }
}
