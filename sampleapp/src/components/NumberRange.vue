<template>
<v-container fluid>
    <v-row>
        <v-col
        cols="12"
        md="12"
        >
            <h1>Range Details</h1>
            <p>This is the range coverage of the existing Ribbon SBC range</p>
            <v-simple-table>
                <template v-slot:default>
                <thead>
                    <tr>
                    <th class="text-left">
                        Number Range
                    </th>
                    <th class="text-left">
                        Total Number
                    </th>
                    <th class="text-left">
                        Ribbon SBC Range
                    </th>
                    <th class="text-left">
                        Total Uncovered Numbers
                    </th>
                    <th class="text-left" width="40%">
                        Result
                    </th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                    <td>123450 to 123460</td>
                    <td>11</td>
                    <td>
                        <v-chip-group>
                        <v-chip
                            class="ma-2"
                            color="success"
                            outlined
                            small
                            >
                            12345
                            </v-chip>
                            </v-chip-group>
                    </td>
                    <td>1</td>
                    <td>
                        <v-progress-linear
                            height="20"
                            value="95"
                            stream
                            color="success"
                            ></v-progress-linear>
                    </td>
                    </tr>
                    <tr>
                    <td>123700 to 123800</td>
                    <td>101</td>
                    <td>
                        <v-chip
                            class="ma-2"
                            color="success"
                            outlined
                            small
                            >
                            123750
                            </v-chip>
                            <v-chip
                            class="ma-2"
                            color="success"
                            outlined
                            small
                            >
                            123950
                            </v-chip>
                        </td>
                    <td>90</td>
                    <td>
                        <div style="width: 100%;display: flex;">
                            <v-progress-linear
                            color="success"
                            value="10"
                            reverse
                            height="20"
                            >
                            </v-progress-linear>
                        <v-progress-linear
                            buffer-value="55"
                            color="success"
                            stream
                            value="30"
                            height="20"
                            >
                            </v-progress-linear>
                        </div>
                        
                    </td>
                    </tr>
                    <tr>
                    <td>123560 to 123560</td>
                    <td>11</td>
                    <td>
                        <v-chip
                            class="ma-2"
                            color="success"
                            outlined
                            small
                            >
                            923
                            </v-chip></td>
                    <td>1000</td>
                    <td>
                        <v-progress-linear
                            height="25"
                            >
                            <strong>Out of range</strong>
                            </v-progress-linear>
                    </td>
                    </tr>
                    <tr>
                    <td>123560 to 123569</td>
                    <td>10</td>
                    <td>
                        <v-chip
                            class="ma-2"
                            color="success"
                            outlined
                            small
                            >
                            12356
                            </v-chip></td>
                    <td>1000</td>
                    <td>
                        <v-progress-linear
                            color="success"
                            value="100"
                            height="20"
                            >
                            <strong>Efficeint</strong>
                            </v-progress-linear>
                    </td>
                    </tr>
                </tbody>
                </template>
            </v-simple-table>
        </v-col>
    </v-row>
    
    <v-row>
        <v-col>
            <h1 style="mt-0">Undefined Ranges</h1>
            <p>This is the list of numbers that not in any existing Ribbon SBC range</p>
            <v-divider></v-divider>
             <v-simple-table>
                <template >
                <thead>
                    <tr>
                    <th class="text-left">
                        Number Range
                    </th>
                    <th class="text-left">
                        Total Number
                    </th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                    <td>45599950 to 45599990</td>
                    <td>41</td>
                    </tr>
                </tbody>
                </template>
            </v-simple-table>
            
        </v-col>
        <v-col cols="12">
            <v-btn
            color="primary" 
            x-large
            block>
                Recalculate Ranges
            </v-btn>
        </v-col>
    </v-row>
</v-container>
</template>

<script>
export default {
    data: () => ({  
        ranges:{
            value: [30, 60],
        },
        fakeRange: [
            {
                range: '123',
                coverage: 1000
            },
            {
                range: '12560',
                coverage: 160
            }
        ],
    })
}
</script>

<style scoped>
.progressbar strong{
    color:white;
}
</style>